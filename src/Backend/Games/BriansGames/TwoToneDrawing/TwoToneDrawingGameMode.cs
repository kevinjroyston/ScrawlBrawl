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
using Common.Code.Extensions;

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
                    Description = "Limit each team member to one color",
                    ResponseType = ResponseType.Boolean,
                    DefaultValue = true,
                },
                new GameModeOptionResponse
                {
                    Description = "Team Size",
                    ResponseType = ResponseType.Integer,
                    DefaultValue = 2,
                    MinValue = 1,
                    MaxValue = 4,
                },
            },
            GetGameDurationEstimates = GetGameDurationEstimates,
        };
        private static IReadOnlyDictionary<GameDuration, TimeSpan> GetGameDurationEstimates(int numPlayers, List<ConfigureLobbyRequest.GameModeOptionRequest> gameModeOptions)
        {
            int maxPossibleTeamCount = 8; // Can go higher than this in extreme circumstances.
            bool useSingleColor = (bool)gameModeOptions[(int)GameModeOptionsEnum.useSingleColor].ValueParsed;
            int numLayers = (int)gameModeOptions[(int)GameModeOptionsEnum.numLayers].ValueParsed;
            if (numLayers * 2 > numPlayers)
            {
                numLayers = numPlayers / 2;
            }

            Dictionary<GameDuration, TimeSpan> estimates = new Dictionary<GameDuration, TimeSpan>();
            foreach (GameDuration duration in Enum.GetValues(typeof(GameDuration)))
            {
                int numRounds = Math.Min(TwoToneDrawingConstants.MaxNumRounds[duration], numPlayers);
                int numDrawingsPerPlayer = Math.Min(TwoToneDrawingConstants.DrawingsPerPlayer[duration], numRounds);

                TimeSpan estimate = TimeSpan.Zero;
                TimeSpan setupTimer = TwoToneDrawingConstants.SetupTimer[duration];
                TimeSpan drawingTimer = TwoToneDrawingConstants.PerDrawingTimer[duration].MultipliedBy(numDrawingsPerPlayer);
                TimeSpan votingTimer = TwoToneDrawingConstants.VotingTimer[duration];

                estimate += votingTimer.MultipliedBy(numRounds);
                estimate += setupTimer;
                estimate += drawingTimer;

                estimates[duration] = estimate;
            }

            return estimates;
        }

        public TwoToneDrawingGameMode(Lobby lobby, List<ConfigureLobbyRequest.GameModeOptionRequest> gameModeOptions, GameDuration duration, bool timerEnabled)
        {
            this.Lobby = lobby;

            int maxPossibleTeamCount = 8; // Can go higher than this in extreme circumstances.
            bool useSingleColor = (bool)gameModeOptions[(int)GameModeOptionsEnum.useSingleColor].ValueParsed;
            int numLayers = (int)gameModeOptions[(int)GameModeOptionsEnum.numLayers].ValueParsed;
            int numPlayers = lobby.GetAllUsers().Count();
            if (numLayers * 2 > numPlayers)
            {
                numLayers = numPlayers / 2;
            }
            int numRounds = Math.Min(TwoToneDrawingConstants.MaxNumRounds[duration], numPlayers);
            int numDrawingsPerPlayer = Math.Min(TwoToneDrawingConstants.DrawingsPerPlayer[duration], numRounds);
            int numTeamsLowerBound = Math.Max(2, 1 * numPlayers / (numRounds * numLayers)); // Lower bound.
            int numTeamsUpperBound = Math.Min(maxPossibleTeamCount, numDrawingsPerPlayer * numPlayers / (numRounds * numLayers)); // Upper bound.
            int numTeams = Math.Max(numTeamsLowerBound, numTeamsUpperBound); // Possible for lower bound to be higher than upper bound. that is okay.


            //int drawingsPerPlayer = numRounds * numLayers * numTeams / numPlayers;

            TimeSpan ? setupTimer = null;
            TimeSpan? drawingTimer = null;
            TimeSpan? votingTimer = null;
            if (timerEnabled)
            {
                setupTimer = TwoToneDrawingConstants.SetupTimer[duration];
                drawingTimer = TwoToneDrawingConstants.PerDrawingTimer[duration].MultipliedBy(numDrawingsPerPlayer);
                votingTimer = TwoToneDrawingConstants.VotingTimer[duration];
            }

            Setup = new Setup_GS(
                lobby: lobby,
                challengeTrackers: this.SubChallenges,
                useSingleColor: useSingleColor,
                numLayersPerTeam: numLayers,
                numTeamsPerPrompt: numTeams,
                numRounds: numRounds,
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
    }
}
