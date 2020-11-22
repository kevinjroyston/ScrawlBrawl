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
            Title = "Chaotic Cooperation",
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
            List<string> randomizedTeamIds = challenge.TeamIdToDrawingMapping.Keys.OrderBy(_=> Rand.Next()).ToList();
            List<List<UserDrawing>> stackedDrawings = randomizedTeamIds.Select(teamId => challenge.TeamIdToDrawingMapping[teamId].Values.Select(drawing => (UserDrawing)drawing).ToList()).ToList();


            return new StackedDrawingVoteAndRevealState(
                lobby: this.Lobby,
                stackedDrawings: stackedDrawings,
                voteCountManager: (Dictionary<User, int> usersToVotes) =>
                {
                    CountVotes(usersToVotes, challenge, randomizedTeamIds);
                },
                votingTime: votingTime)
                {
                    VotingTitle = Invariant($"Which one is the best \"{challenge.Prompt}\"?"),
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
                };
        }
        private void CountVotes(Dictionary<User, int> usersToVotes, ChallengeTracker challenge, List<string> randomizedTeamIds)
        {
            List<List<User>> usersVotedForEachAnswer = new List<List<User>>();
            int mostVotes = 0;
            for (int i = 0; i < randomizedTeamIds.Count; i++)
            {
                usersVotedForEachAnswer.Add(new List<User>());
            }
            foreach (User user in usersToVotes.Keys)
            {
                int indexVotedFor = usersToVotes[user];
                usersVotedForEachAnswer[indexVotedFor].Add(user);
                if (usersVotedForEachAnswer[indexVotedFor].Count > mostVotes)
                {
                    mostVotes = usersVotedForEachAnswer[indexVotedFor].Count;
                }
            }
            for (int i = 0; i < usersVotedForEachAnswer.Count; i++)
            {
                List<User> users = usersVotedForEachAnswer[i];
                if (users.Count == mostVotes)
                {
                    foreach (User drawingUser in challenge.TeamIdToDrawingMapping[randomizedTeamIds[i]].Values.Select(drawing => drawing.Owner))
                    {
                        // 500 points if they helped draw the best drawing.
                        drawingUser.AddScore(TwoToneDrawingConstants.PointsForMakingWinningDrawing);
                    }
                }
                foreach (User user in users)
                {
                    if (users.Count == mostVotes)
                    {
                        // If the user voted for the drawing with the most votes, give them 100 points
                        user.AddScore(TwoToneDrawingConstants.PointsForVotingForWinningDrawing);
                    }
                    else if (challenge.TeamIdToDrawingMapping[randomizedTeamIds[i]].Values.Any(drawing => drawing.Owner == user))
                    {
                        // If the drawing didn't get the most votes and the user voted for themselves subtract points
                        user.AddScore(TwoToneDrawingConstants.PointsToLoseForBadSelfVote);
                    }
                }
            }
            
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
