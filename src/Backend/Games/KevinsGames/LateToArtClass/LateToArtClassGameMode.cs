using Backend.APIs.DataModels.UnityObjects;
using Backend.GameInfrastructure;
using Backend.GameInfrastructure.DataModels;
using Backend.GameInfrastructure.DataModels.States.StateGroups;
using Backend.GameInfrastructure.DataModels.Users;
using Backend.GameInfrastructure.Extensions;
using Backend.Games.KevinsGames.LateToArtClass.DataModels;
using Backend.Games.KevinsGames.LateToArtClass.GameStates;
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
using System.Collections.Concurrent;

namespace Backend.Games.KevinsGames.LateToArtClass
{
    public class LateToArtClassGameMode : IGameMode
    {
        private const int MinPlayers = 4;
        public static GameModeMetadata GameModeMetadata { get; } =
            new GameModeMetadata
            {
                Title = "Late to Art Class",
                GameId = GameModeId.LateToArtClass,
                Description = "Try to get away with forging the art assignment by copying your peer's work.",
                MinPlayers = MinPlayers,
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

            numPlayers = Math.Max(numPlayers, MinPlayers);
            foreach (GameDuration duration in Enum.GetValues(typeof(GameDuration)))
            {
                int numRounds = Math.Min(LateToArtClassConstants.MaxNumRounds[duration], numPlayers);
                int numDrawingsPerUser = Math.Min(LateToArtClassConstants.MaxDrawingsPerPlayer[duration], numRounds - 1);

                numRounds = Math.Min(numRounds, numPlayers* numDrawingsPerUser / LateToArtClassConstants.MinNumPlayersPerRound);

                TimeSpan estimate = TimeSpan.Zero;
                TimeSpan writingTimer = LateToArtClassConstants.WritingTimer[duration];
                TimeSpan drawingTimer = LateToArtClassConstants.DrawingTimer[duration];
                TimeSpan votingTimer = LateToArtClassConstants.VotingTimer[duration];

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
        public LateToArtClassGameMode(Lobby lobby, List<ConfigureLobbyRequest.GameModeOptionRequest> gameModeOptions, StandardGameModeOptions standardOptions)
        {
            this.Lobby = lobby;
            TimeSpan? writingTimer = null;
            TimeSpan? drawingTimer = null;
            TimeSpan? votingTimer = null;
            GameDuration duration = standardOptions.GameDuration;
            if (standardOptions.TimerEnabled)
            {
                writingTimer = LateToArtClassConstants.WritingTimer[duration];
                drawingTimer = LateToArtClassConstants.DrawingTimer[duration];
                votingTimer = LateToArtClassConstants.VotingTimer[duration];
            }
            List<ArtClass> artClasses = new List<ArtClass>();
            int numRounds = Math.Min(LateToArtClassConstants.MaxNumRounds[duration], this.Lobby.GetAllUsers().Count);
            int numDrawingsPerUser = Math.Min(LateToArtClassConstants.MaxDrawingsPerPlayer[duration], numRounds);

            numRounds = Math.Min(numRounds, this.Lobby.GetAllUsers().Count * numDrawingsPerUser / LateToArtClassConstants.MinNumPlayersPerRound);
            int playersPerPrompt = Math.Min(LateToArtClassConstants.MaxNumPlayersPerRound, this.Lobby.GetAllUsers().Count);
            playersPerPrompt = Math.Min(playersPerPrompt, this.Lobby.GetAllUsers().Count * numDrawingsPerUser / numRounds + 1);
            Setup = new Setup_GS(
                lobby: lobby,
                promptsToPopulate: artClasses,
                writingTimeDuration: writingTimer,
                drawingTimeDuration: drawingTimer,
                numDrawingsPerUser: numDrawingsPerUser,
                numRounds: numRounds,
                maxPlayersPerPrompt: playersPerPrompt);
            StateChain CreateGamePlayLoop()
            {
                List<State> stateList = new List<State>();
                List<ArtClass> artClassesList = artClasses.ToList();
                foreach (ArtClass artClass in artClassesList)
                {
                    stateList.Add(GetVotingAndRevealState(artClass, votingTimer));
                }
                stateList.Add(new ScoreBoardGameState(lobby, "Final Scores"));
                StateChain gamePlayChain = new StateChain(states: stateList);
                gamePlayChain.Transition(this.Exit);
                return gamePlayChain;
            }
            this.Entrance.Transition(Setup);
            Setup.Transition(CreateGamePlayLoop);
        }

        private State GetVotingAndRevealState(ArtClass artClass, TimeSpan? votingTime)
        {
            int indexOfImposter = 0;

            // Filter out entries which didnt provide a drawing!
            var randomizedSubset = artClass.UsersToDrawings.Where(kvp => kvp.Value != null).ToList().OrderBy(_ => Rand.Next()).ToList();
            List<User> randomizedUsersToShow = randomizedSubset.Select(kvp => kvp.Key).ToList();
            List<UserDrawing> drawings = randomizedSubset.Select(kvp => kvp.Value).ToList();
            // Copied from will ALWAYS be present based on how construction works. Imposter may not be present though.

            // If we have less drawings than we should, none is an option.
            bool possibleNone = drawings.Count != artClass.UsersToDrawings.Count;
            // If the correct drawing is not present, then none is CORRECT.
            bool noneIsCorrect = possibleNone && !randomizedUsersToShow.Contains(artClass.LateStudent);
            if (!noneIsCorrect)
            {
                indexOfImposter = randomizedUsersToShow.IndexOf(artClass.LateStudent);
            }
            if (possibleNone)
            {
                User owner;
                if (noneIsCorrect)
                {
                    indexOfImposter = drawings.Count;
                    owner = artClass.LateStudent;
                }
                else
                {
                    // Treat owner as the None because it is ez.
                    owner = artClass.Teacher;
                }

                drawings.Add(new UserDrawing()
                {
                    Owner = owner,
                    ShouldHighlightReveal = noneIsCorrect,
                    Drawing = Constants.Drawings.NoneUnityImage,
                    UnityImageRevealOverrides = new UnityObjectOverrides
                    {
                        Title = "None of them",
                    },
                });
                randomizedUsersToShow.Add(owner);
            }

            return new DrawingVoteAndRevealState(
                lobby: this.Lobby,
                drawings: drawings,
                votingTime: votingTime)
            {
                VotingPromptTitle = (user)=> $"Assignment: '{artClass.ArtAssignment}'",
                VotingPromptDescription = (User user)=>$"Who was late to class? {(artClass.LateStudent == user ? "Hint: it was you, your vote does not count towards score here." : string.Empty)}",
                VotingViewOverrides = new UnityViewOverrides
                {
                    Title = $"Who was late to class? Assignment: '{artClass.ArtAssignment}'",
                    Instructions = possibleNone ? "Someone didn't finish so there may not be a late student in this group" : "",
                },
                RevealViewOverrides = new UnityViewOverrides
                {
                    Title = Invariant($"Assignment: '{artClass.ArtAssignment}'"),
                    Instructions = Invariant($"<color=green>{artClass.LateStudent.DisplayName}</color> was late to class! They copied off of <color=blue>{artClass.CopiedFrom.DisplayName}</color>"),
                },
                VoteCountManager = CountVotes(artClass)
            };
        }

        private Action<List<UserDrawing>, IDictionary<User, VoteInfo>> CountVotes(ArtClass artClass)
        {
            return (List<UserDrawing> choices, IDictionary<User, VoteInfo> votes) =>
            {
                // This user knows the answer, their vote doesnt count.
                if (votes.ContainsKey(artClass.LateStudent))
                {
                    votes.Remove(artClass.LateStudent);
                }

                // Votes cast point calculations
                foreach ((User user, VoteInfo vote) in votes)
                {
                    if (((UserDrawing)vote.ObjectsVotedFor[0]).Owner == artClass.LateStudent)
                    {
                        user.ScoreHolder.AddScore(LateToArtClassConstants.PointsForCorrectAnswer, Score.Reason.CorrectAnswer);
                    }

                    if (((UserDrawing)vote.ObjectsVotedFor[0]).Owner == artClass.CopiedFrom)
                    {
                        user.ScoreHolder.AddScore(LateToArtClassConstants.PointsForPartialCorrectAnswer, Score.Reason.LateToArtClass_PartialCorrectAnswer);
                    }
                }

                float votesCastForLateOrCopiedFrom = 0;
                foreach (UserDrawing drawing in choices)
                {
                    // Drawing owner point calculation
                    if (drawing.Owner == artClass.CopiedFrom)
                    {
                        // Just give them some freebies
                        drawing.Owner.ScoreHolder.AddScore(LateToArtClassConstants.FreebiePointsForNormal, Score.Reason.LateToArtClass_GoodNormal);
                    }
                    else if (drawing.Owner != artClass.LateStudent)
                    {
                        // Calculates how good a job a non-imposter did.
                        int playersMisled = drawing.VotesCastForThisObject.Where(vote => vote.UserWhoVoted != artClass.LateStudent).Count();
                        int pointDeduction = playersMisled * LateToArtClassConstants.LostPointsForBadNormal;
                        int netPoints = Math.Max(0, LateToArtClassConstants.FreebiePointsForNormal - pointDeduction);
                        drawing.Owner.ScoreHolder.AddScore(netPoints, Score.Reason.LateToArtClass_GoodNormal);
                    }

                    // Late student scoring is a bit complex, from here onwards
                    if (drawing.Owner == artClass.LateStudent)
                    {
                        votesCastForLateOrCopiedFrom += drawing.VotesCastForThisObject.Where(vote => vote.UserWhoVoted != artClass.LateStudent).Count();
                    }
                    if (drawing.Owner == artClass.CopiedFrom)
                    {
                        // only a half penalty for votes for the copied from.
                        votesCastForLateOrCopiedFrom += 0.5f * drawing.VotesCastForThisObject.Where(vote => vote.UserWhoVoted != artClass.LateStudent).Count();
                    }
                }

                // Calculates how good a job the imposter did.
                float portionVotersCorrectOrPartiallyCorrect = votesCastForLateOrCopiedFrom * 1.0f / votes.Count();
                artClass.LateStudent.ScoreHolder.AddScore((int)Math.Round((1.0 - portionVotersCorrectOrPartiallyCorrect) * LateToArtClassConstants.BonusPointsForGoodImposter), Score.Reason.LateToArtClass_GoodLateStudent);
            };
        }
    }
}
