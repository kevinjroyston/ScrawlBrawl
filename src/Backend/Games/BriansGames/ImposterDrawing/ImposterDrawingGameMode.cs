using Backend.APIs.DataModels.UnityObjects;
using Backend.GameInfrastructure;
using Backend.GameInfrastructure.DataModels;
using Backend.GameInfrastructure.DataModels.States.StateGroups;
using Backend.GameInfrastructure.DataModels.Users;
using Backend.GameInfrastructure.Extensions;
using Backend.Games.BriansGames.ImposterDrawing.DataModels;
using Backend.Games.BriansGames.ImposterDrawing.GameStates;
using Backend.Games.Common;
using Backend.Games.Common.DataModels;
using Backend.Games.Common.GameStates;
using Backend.Games.Common.GameStates.VoteAndReveal;
using Common.Code.Extensions;
using Common.DataModels.Enums;
using Common.DataModels.Requests.LobbyManagement;
using Common.DataModels.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using static System.FormattableString;
using Constants = Common.DataModels.Constants;

namespace Backend.Games.BriansGames.ImposterDrawing
{
    public class ImposterDrawingGameMode : IGameMode
    {
        public static GameModeMetadata GameModeMetadata { get; } =
            new GameModeMetadata
            {
                Title = "Imposter Syndrome",
                GameId = GameModeId.Imposter,
                Description = "Come up with a difference only you'll be able to spot!",
                MinPlayers = 4,
                MaxPlayers = null,
                Attributes = new GameModeAttributes
                {
                   ProductionReady = true,
                },
                Options = new List<GameModeOptionResponse>
                {
                },
                GetGameDurationEstimates = GetGameDurationEstimates,
            };
        private static IReadOnlyDictionary<GameDuration, TimeSpan> GetGameDurationEstimates(int numPlayers, List<ConfigureLobbyRequest.GameModeOptionRequest> gameModeOptions)
        {
            Dictionary<GameDuration, TimeSpan> estimates = new Dictionary<GameDuration, TimeSpan>();
            foreach (GameDuration duration in Enum.GetValues(typeof(GameDuration)))
            {
                int numRounds = Math.Min(ImposterDrawingConstants.MaxNumRounds[duration], numPlayers);
                int numDrawingsPerUser = Math.Min(ImposterDrawingConstants.MaxDrawingsPerPlayer[duration], numRounds-1);

                TimeSpan estimate = TimeSpan.Zero;
                TimeSpan writingTimer = ImposterDrawingConstants.WritingTimer[duration];
                TimeSpan drawingTimer = ImposterDrawingConstants.DrawingTimer[duration];
                TimeSpan votingTimer = ImposterDrawingConstants.VotingTimer[duration];

                estimate += writingTimer;
                estimate += drawingTimer.MultipliedBy(numDrawingsPerUser);
                estimate += votingTimer.MultipliedBy(numRounds);
                estimates[duration] = estimate;
            }

            return estimates;
        }
        private Setup_GS Setup { get; set; }
        private Random Rand { get; } = new Random();
        private Lobby Lobby { get; set; }
        public ImposterDrawingGameMode(Lobby lobby, List<ConfigureLobbyRequest.GameModeOptionRequest> gameModeOptions, StandardGameModeOptions standardOptions)
        {
            this.Lobby = lobby;
            TimeSpan? writingTimer = null;
            TimeSpan? drawingTimer = null;
            TimeSpan? votingTimer = null;
            GameDuration duration = standardOptions.GameDuration;
            if (standardOptions.TimerEnabled)
            {
                writingTimer = ImposterDrawingConstants.WritingTimer[duration];
                drawingTimer = ImposterDrawingConstants.DrawingTimer[duration];
                votingTimer = ImposterDrawingConstants.VotingTimer[duration];
            }
            List<Prompt> prompts = new List<Prompt>();
            int numRounds = Math.Min(ImposterDrawingConstants.MaxNumRounds[duration], this.Lobby.GetAllUsers().Count);
            int numDrawingsPerUser = Math.Min(ImposterDrawingConstants.MaxDrawingsPerPlayer[duration], numRounds - 1);
            Setup = new Setup_GS(
                lobby: lobby,
                promptsToPopulate: prompts,
                writingTimeDuration: writingTimer,
                drawingTimeDuration: drawingTimer,
                numDrawingsPerUser:numDrawingsPerUser,
                numRounds:numRounds,
                maxPlayersPerPrompt:Math.Min(ImposterDrawingConstants.MaxNumPlayersPerRound,this.Lobby.GetAllUsers().Count-1));
            StateChain CreateGamePlayLoop()
            {
                List<State> stateList = new List<State>();
                foreach (Prompt prompt in prompts)
                {
                    stateList.Add(GetImposterLoop(prompt, prompt == prompts.Last()));
                }
                StateChain gamePlayChain = new StateChain(states: stateList);
                gamePlayChain.Transition(this.Exit);
                return gamePlayChain;
            }
            this.Entrance.Transition(Setup);
            Setup.Transition(CreateGamePlayLoop);

            StateChain GetImposterLoop(Prompt prompt, bool lastRound = false)
            {
                return new StateChain(
                    stateGenerator: (int counter) =>
                    {
                        if (counter == 0)
                        {
                            return GetVotingAndRevealState(prompt, (prompt.UsersToDrawings.Values.Any(val=>val==null)), votingTimer);   
                        }
                        if (counter == 1)
                        {
                            if (lastRound)
                            {
                                return new ScoreBoardGameState(lobby, "Final Scores");
                            }
                            else
                            {
                                return new ScoreBoardGameState(lobby);
                            }        
                        }
                        else
                        {
                            return null;
                        }
                    });
            }
        }

        private State GetVotingAndRevealState(Prompt prompt, bool possibleNone, TimeSpan? votingTime)
        {
            int indexOfImposter = 0;
            List<User> randomizedUsersToShow = prompt.UsersToDrawings.Keys.OrderBy(_=>Rand.Next()).ToList();
            List<UserDrawing> drawings = randomizedUsersToShow.Select(user => prompt.UsersToDrawings[user]).ToList();

            List<string> userNames = randomizedUsersToShow.Select(user => user.DisplayName).ToList();
            bool noneIsCorrect = possibleNone && !randomizedUsersToShow.Contains(prompt.Imposter);
            if (!noneIsCorrect)
            {
                indexOfImposter = drawings.IndexOf(prompt.UsersToDrawings[prompt.Imposter]);
            }
            if (possibleNone)
            {
                if (noneIsCorrect)
                {
                    indexOfImposter = drawings.Count;
                    drawings.Add(new UserDrawing()
                    {
                        Owner = prompt.Imposter,
                        Drawing = Constants.Drawings.NoneUnityImage,
                    });
                    randomizedUsersToShow.Add(prompt.Imposter);
                }
                else
                {
                    drawings.Add(new UserDrawing()
                    {
                        Owner = prompt.Owner,
                        Drawing = Constants.Drawings.NoneUnityImage,
                    });
                    randomizedUsersToShow.Add(prompt.Owner);
                }
            }

            return new DrawingVoteAndRevealState(
                lobby: this.Lobby,
                drawings: drawings,
                votingTime: votingTime)
            {
                VotingPromptTitle = (user)=>"Find the Imposter!",
                VotingPromptDescription = (User user)=>$"{((prompt.Owner == user)?($"You created this prompt. Real:'{prompt.RealPrompt}', Imposter:'{prompt.FakePrompt}'"):(!prompt.UsersToDrawings.ContainsKey(user) ? "You didn't draw anything for this prompt" : $"Your prompt was: '{(prompt.Imposter==user?prompt.FakePrompt:prompt.RealPrompt)}'"))}",
                VotingViewOverrides = new UnityViewOverrides
                {
                    Title = "Find the Imposter!",
                    Instructions = possibleNone ? "Someone didn't finish so there may not be an imposter in this group" : "",
                },
                RevealViewOverrides = new UnityViewOverrides
                {
                    Title = Invariant($"<color=green>{prompt.Imposter.DisplayName}</color> was the imposter!"),
                    Instructions = Invariant($"Real: '{prompt.RealPrompt}', Imposter: <color=green>'{prompt.FakePrompt}'</color>"),
                },
                VoteCountManager = CountVotes(prompt)
            };
        }

        private Action<List<UserDrawing>, IDictionary<User, VoteInfo>> CountVotes(Prompt prompt)
        {
            return (List<UserDrawing> choices, IDictionary<User, VoteInfo> votes) =>
            {
                foreach ((User user, VoteInfo vote) in votes)
                {
                    if (((UserDrawing)vote.ObjectsVotedFor[0]).Owner == prompt.Imposter)
                    {
                        user.ScoreHolder.AddScore(ImposterDrawingConstants.PointsForCorrectAnswer, Score.Reason.CorrectAnswer);
                    }
                }

                foreach (UserDrawing drawing in choices)
                {
                    if(drawing.Owner != prompt.Imposter)
                    {
                        // Calculates how good a job a non-imposter did.
                        int playersMisled = drawing.VotesCastForThisObject.Where(vote => vote.UserWhoVoted != drawing.Owner).Count();
                        int pointDeduction = playersMisled * ImposterDrawingConstants.LostPointsForBadNormal;
                        int netPoints = Math.Max(0, ImposterDrawingConstants.FreebiePointsForNormal - pointDeduction);
                        drawing.Owner.ScoreHolder.AddScore(netPoints, Score.Reason.Imposter_GoodNormal);
                    }
                    else
                    {
                        // Calculates how good a job the imposter did.
                        float portionVotersCorrect = (drawing.VotesCastForThisObject.Count() * 1.0f / votes.Count());
                        drawing.Owner.ScoreHolder.AddScore((int)Math.Round(portionVotersCorrect * ImposterDrawingConstants.BonusPointsForGoodImposter), Score.Reason.Imposter_GoodImposter);
                    }
                }
            };
        }
    }
}
