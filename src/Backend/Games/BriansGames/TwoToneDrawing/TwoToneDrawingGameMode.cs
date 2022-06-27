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
        private const int MinPlayers = 4;
        private bool UseSingleColor;
        public static GameModeMetadata GameModeMetadata { get; } = new GameModeMetadata
        {
            Title = "Chaotic Collaboration",
            GameId = GameModeId.ChaoticCoop,
            Description = "Blindly collaborate on a drawing with unknown teammates.",
            MinPlayers = MinPlayers,
            MaxPlayers = CommonGameConstants.MAX_PLAYERS,
            Attributes = new GameModeAttributes
                {
                    ProductionReady = true,
                },
            Options = new List<GameModeOptionResponse>
            {
                new GameModeOptionResponse
                {
                    Description = "Limit each artist to one color",
                    ResponseType = ResponseType.Boolean,
                    DefaultValue = false,
                },
                new GameModeOptionResponse
                {
                    Description = "Players per team",
                    ResponseType = ResponseType.Integer,
                    DefaultValue = 2,
                    MinValue = 2,
                    MaxValue = 4,
                },
            },
            GetGameDurationEstimates = GetGameDurationEstimates,
            GetTutorialHiddenClasses = GetTutorialHiddenClasses,
        };
        private static List<string>  GetTutorialHiddenClasses(List<ConfigureLobbyRequest.GameModeOptionRequest> gameModeOptions)
        {
            bool useSingleColor = (bool)gameModeOptions[(int)GameModeOptionsEnum.useSingleColor].ValueParsed;
            int numLayers = (int)gameModeOptions[(int)GameModeOptionsEnum.numLayers].ValueParsed;

            List<string> toReturn = new List<string>();
            toReturn.Add(useSingleColor ? "Tut-Chaotic-Layers": "Tut-Chaotic-Colors");
            toReturn.Add(numLayers == 2 ? "Tut-Chaotic-MultiPlayer" : "Tut-Chaotic-TwoPlayer");
            return toReturn;
        }
        private static IReadOnlyDictionary<GameDuration, TimeSpan> GetGameDurationEstimates(int numPlayers, List<ConfigureLobbyRequest.GameModeOptionRequest> gameModeOptions)
        {
            bool useSingleColor = (bool)gameModeOptions[(int)GameModeOptionsEnum.useSingleColor].ValueParsed;
            Dictionary<GameDuration, TimeSpan> estimates = new Dictionary<GameDuration, TimeSpan>();

            numPlayers = Math.Max(numPlayers, MinPlayers);
            foreach (GameDuration duration in Enum.GetValues(typeof(GameDuration)))
            {
                int numRounds = Math.Min(TwoToneDrawingConstants.MaxNumRounds[duration], numPlayers);
                int numDrawingsPerPlayer = Math.Min(TwoToneDrawingConstants.DrawingsPerPlayer[duration], numRounds); 
                
                int maxPossibleTeamCount = 6; // Can go higher than this in extreme circumstances.
                int numLayers = (int)gameModeOptions[(int)GameModeOptionsEnum.numLayers].ValueParsed;
                if (numLayers * 2 > numPlayers)
                {
                    numLayers = numPlayers / 2;
                }
                int numTeamsLowerBound = Math.Max(2, 1 * numPlayers / (numRounds * numLayers)); // Lower bound.
                int numTeamsUpperBound = Math.Min(maxPossibleTeamCount, numDrawingsPerPlayer * numPlayers / (numRounds * numLayers)); // Upper bound.
                int numTeams = Math.Max(numTeamsLowerBound, numTeamsUpperBound); // Possible for lower bound to be higher than upper bound. that is okay.
                numDrawingsPerPlayer = Math.Max(Math.Min(numDrawingsPerPlayer, (int)Math.Ceiling((numTeams * numRounds * numLayers * 1.0) / numPlayers)), 1);

                TimeSpan estimate = TimeSpan.Zero;
                TimeSpan setupTimer = TwoToneDrawingConstants.SetupTimer[duration];
                TimeSpan drawingTimer = TwoToneDrawingConstants.PerDrawingTimer[duration].MultipliedBy(numDrawingsPerPlayer);
                TimeSpan votingTimer = TwoToneDrawingConstants.VotingTimer[duration];

                estimate += votingTimer.MultipliedBy(numRounds);
                estimate += setupTimer.MultipliedBy(useSingleColor ? 1 : 1.5f);
                estimate += drawingTimer;

                estimates[duration] = estimate;
            }

            return estimates;
        }

        public TwoToneDrawingGameMode(Lobby lobby, List<ConfigureLobbyRequest.GameModeOptionRequest> gameModeOptions, StandardGameModeOptions standardOptions)
        {
            this.Lobby = lobby;
            GameDuration duration = standardOptions.GameDuration;

            int numPlayers = lobby.GetAllUsers().Count();

            int maxPossibleTeamCount = 6; // Can go higher than this in extreme circumstances.
            UseSingleColor = (bool)gameModeOptions[(int)GameModeOptionsEnum.useSingleColor].ValueParsed;
            int numLayers = (int)gameModeOptions[(int)GameModeOptionsEnum.numLayers].ValueParsed;
            if (numLayers * 2 > numPlayers)
            {
                numLayers = numPlayers / 2;
            }
            int numRounds = Math.Min(TwoToneDrawingConstants.MaxNumRounds[duration], numPlayers);
            int numDrawingsPerPlayer = Math.Min(TwoToneDrawingConstants.DrawingsPerPlayer[duration], numRounds);
            int numTeamsLowerBound = Math.Max(2, 1 * numPlayers / (numRounds * numLayers)); // Lower bound.
            int numTeamsUpperBound = Math.Min(maxPossibleTeamCount, numDrawingsPerPlayer * numPlayers / (numRounds * numLayers)); // Upper bound.
            int numTeams = Math.Max(numTeamsLowerBound, numTeamsUpperBound); // Possible for lower bound to be higher than upper bound. that is okay.
            numDrawingsPerPlayer = Math.Max(Math.Min(numDrawingsPerPlayer, (int)Math.Ceiling((numTeams * numRounds * numLayers * 1.0) / numPlayers)), 1);

            TimeSpan ? setupTimer = null;
            TimeSpan? perDrawingTimer = null;
            TimeSpan? votingTimer = null;
            if (standardOptions.TimerEnabled)
            {
                setupTimer = TwoToneDrawingConstants.SetupTimer[duration].MultipliedBy(UseSingleColor ? 1 : 1.5f);
                perDrawingTimer = TwoToneDrawingConstants.PerDrawingTimer[duration];
                votingTimer = TwoToneDrawingConstants.VotingTimer[duration];
            }

            Setup = new Setup_GS(
                lobby: lobby,
                challengeTrackers: this.SubChallenges,
                useSingleColor: UseSingleColor,
                numLayersPerTeam: numLayers,
                numTeamsPerPrompt: numTeams,
                numRounds: numRounds,
                setupTimer: setupTimer,
                perDrawingTimer: perDrawingTimer,
                numDrawingsPerPlayer: numDrawingsPerPlayer);

            StateChain GamePlayLoopGenerator()
            {
                List<ChallengeTracker> challenges = SubChallenges.Keys.OrderBy(_ => Rand.Next()).ToList();
                List<State> stateList = new List<State>();
                int currentRound = 0;
                foreach (ChallengeTracker challenge in challenges)
                {
                    currentRound++;
                    stateList.Add(GetVotingAndRevealState(
                        challenge,
                        votingTimer,
                        new UnityRoundDetails
                        {
                            CurrentRound = currentRound,
                            TotalRounds = challenges.Count
                        }));
                }
                stateList.Add(new ScoreBoardGameState(lobby));
                StateChain gamePlayChain = new StateChain(states: stateList);
                gamePlayChain.Transition(this.Exit);
                return gamePlayChain;
            }

            Setup.Transition(GamePlayLoopGenerator);
            this.Entrance.Transition(Setup);
        }

        private State GetVotingAndRevealState(ChallengeTracker challenge, TimeSpan? votingTime, UnityRoundDetails roundDetails)
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
                roundDetails: roundDetails,
                votingTime: votingTime)
                {
                    VotingViewOverrides = new UnityViewOverrides
                    {
                        Title = Invariant($"Which one is the best \"{challenge.Prompt}\"?"),
                        Instructions =  UseSingleColor ? null : $"{string.Join(" | ", ((IEnumerable<string>)challenge.Colors).Reverse().ToArray())}",
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

                // This is probably just equal to num players -1.
                int totalOtherVotes = (choices.Sum((drawingStack) => drawingStack.VotesCastForThisObject.Count)) - 1;
                // Points for voting with crowd.
                foreach (UserDrawingStack<TeamUserDrawing> drawingStack in choices)
                {
                    // Gives a percentage of voting with crowd points. Linear with the percentage of other players who agreed with you.
                    int scorePerPlayer = (int)(TwoToneDrawingConstants.PointsForVotingForWinningDrawing * ((drawingStack.VotesCastForThisObject.Count - 1) / 1.0 / totalOtherVotes));
                    foreach (User userWhoVoted in drawingStack.VotesCastForThisObject.Select(vote => vote.UserWhoVoted))
                    {
                        userWhoVoted.ScoreHolder.AddScore(scorePerPlayer, Score.Reason.VotedWithCrowd);
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
