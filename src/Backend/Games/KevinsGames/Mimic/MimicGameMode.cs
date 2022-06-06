using Backend.GameInfrastructure.DataModels.States.GameStates;
using Backend.GameInfrastructure.Extensions;
using Backend.Games.KevinsGames.Mimic.GameStates;
using Common.DataModels.Requests.LobbyManagement;
using System.Collections.Generic;
using Backend.Games.Common.GameStates;
using Backend.GameInfrastructure.DataModels.Users;
using System;
using System.Linq;
using Backend.GameInfrastructure.DataModels.States.StateGroups;
using System.Collections.Concurrent;
using Backend.Games.Common.DataModels;
using Backend.Games.KevinsGames.Mimic.DataModels;
using Backend.GameInfrastructure.DataModels;
using Backend.Games.Common.GameStates.VoteAndReveal;
using Backend.Games.Common;
using Common.DataModels.Responses;
using Backend.GameInfrastructure;
using Common.Code.Extensions;
using Common.DataModels.Enums;
using Backend.APIs.DataModels.UnityObjects;

namespace Backend.Games.KevinsGames.Mimic
{
    public class MimicGameMode : IGameMode
    {
        private ConcurrentBag<UserDrawing> Drawings { get; set; } = new ConcurrentBag<UserDrawing>();
        private GameState Setup { get; set; }
        private Lobby Lobby { get; set; }
        private Random Rand { get; } = new Random();
        private const int MinPlayers = 3;
        public static GameModeMetadata GameModeMetadata { get; } = new GameModeMetadata
        {
            Title = "Mimic",
            GameId = GameModeId.Mimic,
            Description = "Test your drawing and memory skills",
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
                        Description = "Memorization difficulty",
                        ResponseType = ResponseType.Integer,
                        DefaultValue = 3,
                        MinValue = 1,
                        MaxValue = 10,
                    },
                },
            GetGameDurationEstimates = GetGameDurationEstimates,
        };
        private static IReadOnlyDictionary<GameDuration, TimeSpan> GetGameDurationEstimates(int numPlayers, List<ConfigureLobbyRequest.GameModeOptionRequest> gameModeOptions)
        {
            int numStartingDrawingsPerUser = 1;

            numPlayers = Math.Max(numPlayers, MinPlayers);

            Dictionary<GameDuration, TimeSpan> estimates = new Dictionary<GameDuration, TimeSpan>();
            foreach (GameDuration duration in Enum.GetValues(typeof(GameDuration)))
            {
                int numRounds = Math.Min(MimicConstants.MaxRounds[duration], numPlayers);

                TimeSpan estimate = TimeSpan.Zero;
                TimeSpan votingTimer = MimicConstants.VotingTimer[duration];
                TimeSpan drawingTimer = MimicConstants.DrawingTimer[duration];
                TimeSpan extendedDrawingTimer = drawingTimer.MultipliedBy(MimicConstants.MimicTimerMultiplier);

                estimate += (MimicConstants.MemorizeTimerLength.Add(extendedDrawingTimer).Add(votingTimer)).MultipliedBy(numRounds);
                estimate += drawingTimer.MultipliedBy(numStartingDrawingsPerUser);

                estimates[duration] = estimate;
            }

            return estimates;
        }
        public MimicGameMode(Lobby lobby, List<ConfigureLobbyRequest.GameModeOptionRequest> gameModeOptions, StandardGameModeOptions standardOptions)
        {
            GameDuration duration = standardOptions.GameDuration;
            int numStartingDrawingsPerUser = 1;
            int maxDrawingsBeforeVoteInput = (int)gameModeOptions[(int)GameModeOptions.MaxDrawingsBeforeVote].ValueParsed;
            int maxVoteDrawings = 12; // Everybody's drawing should show up, but it gets a little prohibitive past 12 so limit it here.
            TimeSpan? drawingTimer = null;
            TimeSpan? votingTimer = null;
            if (standardOptions.TimerEnabled)
            {
                drawingTimer = MimicConstants.DrawingTimer[duration];
                votingTimer = MimicConstants.VotingTimer[duration];
            }
            TimeSpan? extendedDrawingTimer = drawingTimer.MultipliedBy(MimicConstants.MimicTimerMultiplier);
            int numPlayers = lobby.GetAllUsers().Count();
            int numRounds = Math.Min(MimicConstants.MaxRounds[duration], numPlayers);

            this.Lobby = lobby;

            Setup = new Setup_GS(
                lobby: lobby,
                drawings: Drawings,
                numDrawingsPerUser: numStartingDrawingsPerUser,
                drawingTimeDuration: drawingTimer);
            List<UserDrawing> randomizedDrawings = new List<UserDrawing>();
            Setup.AddExitListener(() =>
            {
                randomizedDrawings = this.Drawings
                .OrderBy(_ => Rand.Next())
                .ToList()
                .Take(numRounds) // Limit number of rounds based on game duration.
                .ToList();
            });     
            StateChain CreateGamePlayLoop()
            {
                bool timeToShowScores = true;
                StateChain gamePlayLoop = new StateChain(stateGenerator: (int counter) =>
                {
                    if(randomizedDrawings.Count>0)
                    {
                        StateChain CreateMultiRoundLoop()
                        {
                            int maxDrawingsBeforeVote = Math.Min(maxDrawingsBeforeVoteInput, randomizedDrawings.Count);
                            if (randomizedDrawings.Count == 0)
                            {
                                throw new Exception("Something went wrong while setting up the game");
                            }
                            List<RoundTracker> roundTrackers = new List<RoundTracker>();
                            return new StateChain(stateGenerator: (int counter) =>
                            {
                                if (counter < maxDrawingsBeforeVote)
                                {
                                    UserDrawing originalDrawing = randomizedDrawings.First();
                                    randomizedDrawings.RemoveAt(0);

                                    RoundTracker drawingsRoundTracker = new RoundTracker();
                                    roundTrackers.Add(drawingsRoundTracker);
                                    drawingsRoundTracker.originalDrawer = originalDrawing.Owner;
                                    drawingsRoundTracker.UsersToUserDrawings[originalDrawing.Owner] = originalDrawing;

                                    DisplayOriginal_GS displayGS = new DisplayOriginal_GS(
                                        lobby: lobby,
                                        displayTimeDuration: MimicConstants.MemorizeTimerLength,
                                        displayDrawing: originalDrawing);
                                    CreateMimics_GS mimicsGS = new CreateMimics_GS(
                                        lobby: lobby,
                                        roundTracker: drawingsRoundTracker,
                                        drawingTimeDuration: extendedDrawingTimer
                                        );
                                    mimicsGS.AddExitListener(() =>
                                    {
                                        List<User> randomizedUsersToDisplay = new List<User>();
                                        List<User> randomizedKeys = drawingsRoundTracker.UsersToUserDrawings.Keys.OrderBy(_ => Rand.Next()).ToList();
                                        for (int i = 0; i < maxVoteDrawings && i < randomizedKeys.Count; i++)
                                        {
                                            randomizedUsersToDisplay.Add(randomizedKeys[i]);
                                        }
                                        if (!randomizedUsersToDisplay.Contains(drawingsRoundTracker.originalDrawer))
                                        {
                                            randomizedUsersToDisplay.RemoveAt(0);
                                            randomizedUsersToDisplay.Add(drawingsRoundTracker.originalDrawer);
                                        }
                                        randomizedUsersToDisplay = randomizedUsersToDisplay.OrderBy(_ => Rand.Next()).ToList();
                                        drawingsRoundTracker.UsersToDisplay = randomizedUsersToDisplay;
                                    });
                                    return new StateChain(states: new List<State>() { displayGS, mimicsGS }, exit: null);
                                }
                                else if (counter < maxDrawingsBeforeVote * 2)
                                {
                                    return GetVotingAndRevealState(roundTrackers[counter - maxDrawingsBeforeVote], votingTimer);
                                }
                                else
                                {
                                    return null;
                                }
                            });
                        }
                        return CreateMultiRoundLoop();
                    }
                    else
                    {
                        if (timeToShowScores)
                        {
                            timeToShowScores = false;
                            return new ScoreBoardGameState(
                                lobby: lobby,
                                title: "Final Scores");
                        }
                        else
                        {
                            //Ends the chain
                            return null;
                        }
                    }
                });
                gamePlayLoop.Transition(this.Exit);
                return gamePlayLoop;
            }
            
            this.Entrance.Transition(Setup);
            Setup.Transition(CreateGamePlayLoop);
        }
        private State GetVotingAndRevealState(RoundTracker roundTracker, TimeSpan? votingTime)
        {
            List<UserDrawing> drawings = roundTracker.UsersToDisplay.Select(user => roundTracker.UsersToUserDrawings[user]).ToList();
            int indexOfOriginal = roundTracker.UsersToDisplay.IndexOf(roundTracker.originalDrawer);
            drawings[indexOfOriginal].ShouldHighlightReveal = true;

            return new BlurredImageVoteAndRevealState(
                lobby: this.Lobby,
                drawings: drawings,
                blurRevealDelay: MimicConstants.BlurDelay,
                blurRevealLength: MimicConstants.BlurLength,
                votingTime: votingTime)
            {
                VotingViewOverrides = new UnityViewOverrides
                {
                    Title = "Find the Original!",
                },
                VoteCountManager=CountVotes(roundTracker)
            };
        }
        private Action<List<UserDrawing>,IDictionary<User,VoteInfo>> CountVotes(RoundTracker roundTracker)
        {
            return (List<UserDrawing> drawings, IDictionary<User, VoteInfo> votes) =>
            {
                foreach ((User user, VoteInfo vote) in votes)
                {
                    User userVotedFor = ((UserDrawing)vote.ObjectsVotedFor[0]).Owner;
                    if (userVotedFor == roundTracker.originalDrawer)
                    {
                        user.ScoreHolder.AddScore(
                            CommonHelpers.PointsForSpeed(
                                maxPoints: MimicConstants.PointsForCorrectPick(drawings.Count),
                                minPoints: MimicConstants.PointsForCorrectPick(drawings.Count) / 10,
                                startTime: MimicConstants.BlurDelay,
                                endTime: MimicConstants.BlurDelay + MimicConstants.BlurLength,
                                secondsTaken: vote.TimeTakenInMs / 1000.0f),
                            Score.Reason.CorrectAnswerSpeed);
                    }
                    else
                    {
                        userVotedFor.ScoreHolder.AddScore(MimicConstants.PointsForVote, Score.Reason.ReceivedVotes);
                    }
                }
            };
        }
    } 
}
