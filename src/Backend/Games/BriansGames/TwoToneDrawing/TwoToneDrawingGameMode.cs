using Backend.GameInfrastructure.DataModels;
using Backend.GameInfrastructure.DataModels.States.GameStates;
using Backend.GameInfrastructure.DataModels.States.StateGroups;
using Backend.GameInfrastructure.DataModels.Users;
using Backend.GameInfrastructure.Extensions;
using Backend.Games.BriansGames.TwoToneDrawing.DataModels;
using Backend.Games.BriansGames.TwoToneDrawing.GameStates;
using Backend.Games.Common;
using Backend.Games.Common.DataModels;
using Backend.Games.Common.GameStates;
using Backend.Games.Common.GameStates.VoteAndReveal;
using Common.DataModels.Requests.LobbyManagement;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using static Backend.Games.BriansGames.TwoToneDrawing.DataModels.ChallengeTracker;
using static System.FormattableString;
using Backend.GameInfrastructure;
using Common.DataModels.Responses;
using Common.DataModels.Enums;
using Backend.Games.Common.DataModels.UserCreatedObjects.UserCreatedUnityObjects;
using Backend.APIs.DataModels.UnityObjects;

namespace Backend.Games.BriansGames.TwoToneDrawing
{
    public class TwoToneDrawingGameMode : IGameMode
    {
        private ConcurrentDictionary<ChallengeTracker, object> SubChallenges { get; set; } = new ConcurrentDictionary<ChallengeTracker, object>();
        private Lobby Lobby {get; set;}
        private GameState Setup { get; set; }
        private List<GameState> Gameplay { get; set; } = new List<GameState>();
        private List<GameState> Scoreboards { get; set; } = new List<GameState>();
        private List<GameState> VoteReveals { get; set; } = new List<GameState>();
        private Random Rand { get; } = new Random();

        public static GameModeMetadata GameModeMetadata { get; } = new GameModeMetadata
        {
            Title = "Chaotic Collaboration",
            GameId = GameModeId.ChaoticCoop,
            Description = "Blindly collaborate on a drawing with unknown teammates.",
            MinPlayers = 4,
            MaxPlayers = null,
            Attributes = new GameModeAttributes
                {
                    ProductionReady = true,
                },
            Options = new List<GameModeOptionResponse>
            {
                new GameModeOptionResponse
                {
                    Description = "Max number of colors per team",
                    ResponseType = ResponseType.Integer,
                    DefaultValue = 2,
                    MinValue = 1,
                    MaxValue = 10,
                },
                new GameModeOptionResponse
                {
                    Description = "Max number of teams per prompt",
                    ResponseType = ResponseType.Integer,
                    DefaultValue = 4,
                    MinValue = 2,
                    MaxValue = 20,
                },
                new GameModeOptionResponse
                {
                    Description = "Show other colors",
                    DefaultValue = true,
                    ResponseType = ResponseType.Boolean
                },
                new GameModeOptionResponse
                {
                    Description = "Length of the game (10 for longest 1 for shortest 0 for no timer)",
                    ResponseType = ResponseType.Integer,
                    DefaultValue = 5,
                    MinValue = 0,
                    MaxValue = 10,
                }
            }
        };

        public TwoToneDrawingGameMode(Lobby lobby, List<ConfigureLobbyRequest.GameModeOptionRequest> gameModeOptions)
        {
            ValidateOptions(lobby, gameModeOptions);
            this.Lobby = lobby;

            int numColors = (int)gameModeOptions[(int)GameModeOptionsEnum.numColors].ValueParsed;
            int numTeams = (int)gameModeOptions[(int)GameModeOptionsEnum.numTeams].ValueParsed;
            bool showOtherColors = (bool)gameModeOptions[(int)GameModeOptionsEnum.showOtherColors].ValueParsed;
            int gameLength = (int)gameModeOptions[(int)GameModeOptionsEnum.GameLength].ValueParsed;
            TimeSpan? setupTimer = null;
            TimeSpan? drawingTimer = null;
            TimeSpan? votingTimer = null;
            if (gameLength > 0)
            {
                setupTimer = CommonHelpers.GetTimerFromLength(
                    length: (double)gameLength,
                    minTimerLength: TwoToneDrawingConstants.SetupTimerMin,
                    aveTimerLength: TwoToneDrawingConstants.SetupTimerAve,
                    maxTimerLength: TwoToneDrawingConstants.SetupTimerMax);
                drawingTimer = CommonHelpers.GetTimerFromLength(
                    length: (double)gameLength,
                    minTimerLength: TwoToneDrawingConstants.PerDrawingTimerMin,
                    aveTimerLength: TwoToneDrawingConstants.PerDrawingTimerAve,
                    maxTimerLength: TwoToneDrawingConstants.PerDrawingTimerMax);
                votingTimer = CommonHelpers.GetTimerFromLength(
                    length: (double)gameLength,
                    minTimerLength: TwoToneDrawingConstants.VotingTimerMin,
                    aveTimerLength: TwoToneDrawingConstants.VotingTimerAve,
                    maxTimerLength: TwoToneDrawingConstants.VotingTimerMax);
            }

            Setup = new Setup_GS(
                lobby: lobby,
                challengeTrackers: this.SubChallenges,
                numColorsPerTeam: numColors,
                numTeamsPerPrompt: numTeams,
                showColors: showOtherColors,
                setupTimer: setupTimer,
                drawingTimer: drawingTimer);

            StateChain GamePlayLoopGenerator()
            {
                List<ChallengeTracker> challenges = SubChallenges.Keys.OrderBy(_ => Rand.Next()).ToList();
                StateChain chain = new StateChain(
                stateGenerator: (int counter) =>
                {
                    if (counter < challenges.Count)
                    {
                        return new StateChain(stateGenerator: (int i) => {
                            switch(i)
                            {
                                case 0: return GetVotingAndRevealState(challenges[counter], votingTimer);
                                case 1: return ((counter == challenges.Count - 1) ? new ScoreBoardGameState(lobby, "Final Scores") : new ScoreBoardGameState(lobby));
                                default: return null;
                            }});
                    }
                    else
                    {
                        return null;
                    }
                });
                chain.Transition(this.Exit);
                return chain;
            }

            Setup.Transition(GamePlayLoopGenerator);
            this.Entrance.Transition(Setup);
        }

        private State GetVotingAndRevealState(ChallengeTracker challenge, TimeSpan? votingTime)
        {
            AssignUsersToChallenge(challenge);
            List<string> randomizedTeamIds = challenge.TeamIdToDrawingMapping.Keys.OrderBy(_ => Rand.Next()).ToList();
            IReadOnlyList<string> orderedColors = challenge.Colors.AsReadOnly();
            List<UserDrawingStack<TeamUserDrawing>> stackedDrawings = randomizedTeamIds.Select(
                teamId => new UserDrawingStack<TeamUserDrawing>
                {
                    UserDrawings = orderedColors.Select(color=>challenge.TeamIdToDrawingMapping[teamId][color]).ToList()
                }).ToList();

            return new StackedDrawingVoteAndRevealState<TeamUserDrawing>(
                lobby: this.Lobby,
                stackedDrawings: stackedDrawings,
                votingTime: votingTime)
                {
                    VotingViewOverrides = new UnityViewOverrides
                    {
                        Title = Invariant($"Which one is the best \"{challenge.Prompt}\"?"),
                    },
                    PromptAnswerAddOnGenerator = (User user, int answer) =>
                    {
                        if (challenge.TeamIdToDrawingMapping[randomizedTeamIds[answer]].Values.Any(drawing => drawing.Owner == user))
                        {
                            return " - You helped make this";
                        }
                        else
                        {
                            return "";
                        }
                    },
                    VoteCountManager = CountVotes(challenge)
                };
        }
        private Action<List<UserDrawingStack<TeamUserDrawing>>,IDictionary<User,VoteInfo>> CountVotes(ChallengeTracker challenge)
        {
            return (List<UserDrawingStack<TeamUserDrawing>> choices, IDictionary<User, VoteInfo> votes) =>
            {
                foreach ((User user, VoteInfo vote) in votes)
                {
                    List<User> drawingStackOwners =((UserDrawingStack<TeamUserDrawing>)vote.ObjectsVotedFor[0]).UserDrawings.Select(drawing => drawing.Owner).ToList();
                    foreach(User votedForUser in drawingStackOwners)
                    {
                        if(votedForUser != user)
                        {
                            votedForUser.ScoreHolder.AddScore(TwoToneDrawingConstants.PointsPerVote, Score.Reason.ReceivedVotes);
                        }
                    }
                }
                int mostVotes = choices.Max((drawingStack) => drawingStack.VotesCastForThisObject.Count);
                foreach(UserDrawingStack<TeamUserDrawing> drawingStack in choices.Where((drawingStack)=>drawingStack.VotesCastForThisObject.Count == mostVotes))
                {
                    foreach(User userWhoVoted in drawingStack.VotesCastForThisObject.Select(vote => vote.UserWhoVoted))
                    {
                        userWhoVoted.ScoreHolder.AddScore(TwoToneDrawingConstants.PointsForVotingForWinningDrawing, Score.Reason.VotedWithCrowd);
                    }
                }
            };
        }
        private void AssignUsersToChallenge(ChallengeTracker challenge)
        {
            foreach (var kvp in challenge.UserSubmittedDrawings)
            {
                challenge.TeamIdToDrawingMapping.AddOrUpdate(kvp.Value.TeamId, _ => new ConcurrentDictionary<string, TeamUserDrawing>(new Dictionary<string, TeamUserDrawing> { { kvp.Value.Color, kvp.Value } }),
                    (key, currentDictionary) =>
                    {
                        currentDictionary[kvp.Value.Color] = kvp.Value;
                        return currentDictionary;
                    });
                challenge.TeamIdToUsersWhoVotedMapping.GetOrAdd(kvp.Value.TeamId, _ => new ConcurrentBag<User>());
            }
        }
        public void ValidateOptions(Lobby lobby, List<ConfigureLobbyRequest.GameModeOptionRequest> gameModeOptions)
        {
            // None
        }
    }
}
