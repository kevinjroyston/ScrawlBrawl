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

namespace Backend.Games.KevinsGames.Mimic
{
    public class MimicGameMode : IGameMode
    {
        private ConcurrentBag<UserDrawing> Drawings { get; set; } = new ConcurrentBag<UserDrawing>();
        private GameState Setup { get; set; }
        private Lobby Lobby { get; set; }
        private Random Rand { get; } = new Random();
        public static GameModeMetadata GameModeMetadata { get; } = new GameModeMetadata
        {
            Title = "Mimic",
            GameId = GameModeId.Mimic,
            Description = "Test your drawing and memory skills",
            MinPlayers = 3,
            MaxPlayers = null,
            Attributes = new GameModeAttributes
                {
                    ProductionReady = true,
                },
            Options = new List<GameModeOptionResponse>
                {
                    new GameModeOptionResponse
                    {
                        Description = "Number of starting drawings from each person",
                        ResponseType = ResponseType.Integer,
                        DefaultValue = 2,
                        MinValue = 1,
                        MaxValue = 30,
                    },
                    new GameModeOptionResponse
                    {
                        Description = "Number of drawings before players are asked to vote",
                        ResponseType = ResponseType.Integer,
                        DefaultValue = 5,
                        MinValue = 1,
                        MaxValue = 30,
                    },
                    new GameModeOptionResponse
                    {
                        Description = "Number of sets of drawings per game",
                        ResponseType = ResponseType.Integer,
                        DefaultValue = 3,
                        MinValue = 1,
                        MaxValue = 50,
                    },
                    new GameModeOptionResponse
                    {
                        Description = "Max number of drawings to display for voting",
                        ResponseType = ResponseType.Integer,
                        DefaultValue = 10,
                        MinValue = 2,
                        MaxValue = 36,
                    },
                    new GameModeOptionResponse
                    {
                        Description = "Length of the game (10 for longest 1 for shortest 0 for no timer)",
                        ResponseType = ResponseType.Integer,
                        DefaultValue = 5,
                        MinValue = 0,
                        MaxValue = 10,
                    },
                }
        };
        public MimicGameMode(Lobby lobby, List<ConfigureLobbyRequest.GameModeOptionRequest> gameModeOptions)
        {
            ValidateOptions(gameModeOptions);
            int numStartingDrawingsPerUser = (int)gameModeOptions[(int)GameModeOptions.NumStartingDrawingsPerUser].ValueParsed;
            int maxDrawingsBeforeVoteInput = (int)gameModeOptions[(int)GameModeOptions.MaxDrawingsBeforeVote].ValueParsed;
            int numSets = (int)gameModeOptions[(int)GameModeOptions.NumSets].ValueParsed;
            int maxVoteDrawings = (int)gameModeOptions[(int)GameModeOptions.MaxVoteDrawings].ValueParsed;
            int gameLength = (int)gameModeOptions[(int)GameModeOptions.GameLength].ValueParsed;
            TimeSpan? setupTimer = null;
            TimeSpan? drawingTimer = null;
            TimeSpan? votingTimer = null;
            if (gameLength > 0)
            {
                drawingTimer = CommonHelpers.GetTimerFromLength(
                    length: (double)gameLength,
                    minTimerLength: MimicConstants.DrawingTimerMin,
                    aveTimerLength: MimicConstants.DrawingTimerAve,
                    maxTimerLength: MimicConstants.DrawingTimerMax);
                votingTimer = CommonHelpers.GetTimerFromLength(
                    length: (double)gameLength,
                    minTimerLength: MimicConstants.VotingTimerMin,
                    aveTimerLength: MimicConstants.VotingTimerAve,
                    maxTimerLength: MimicConstants.VotingTimerMax);
            }
            TimeSpan? extendedDrawingTimer = drawingTimer.MultipliedBy(MimicConstants.MimicTimerMultiplier);

            this.Lobby = lobby;

            Setup = new Setup_GS(
                lobby: lobby,
                drawings: Drawings,
                numDrawingsPerUser: numStartingDrawingsPerUser,
                drawingTimeDuration: drawingTimer);
            List<UserDrawing> randomizedDrawings = new List<UserDrawing>();
            Setup.AddExitListener(() =>
            {
                randomizedDrawings = this.Drawings.OrderBy(_ => Rand.Next()).ToList();
            });     
            StateChain CreateGamePlayLoop()
            {
                bool timeToShowScores = true;
                StateChain gamePlayLoop = new StateChain(stateGenerator: (int counter) =>
                {
                    if(counter<numSets && randomizedDrawings.Count>0)
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
                                    drawingsRoundTracker.UsersToUserDrawings.AddOrUpdate(originalDrawing.Owner, originalDrawing, (User user, UserDrawing drawing) => randomizedDrawings.First());

                                    DisplayOriginal_GS displayGS = new DisplayOriginal_GS(
                                        lobby: lobby,
                                        displayTimeDuration: TimeSpan.FromSeconds(MimicConstants.MemorizeTimerLength),
                                        displayDrawing: originalDrawing);
                                    CreateMimics_GS mimicsGS = new CreateMimics_GS(
                                        lobby: lobby,
                                        roundTracker: roundTrackers.Last(),
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
                                    return new StateChain(states: new List<State>() { displayGS, mimicsGS }, entrance: null, exit: null);
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

            return new BlurredImageVoteAndRevealState(
                lobby: this.Lobby,
                drawings: drawings,
                blurRevealDelay: MimicConstants.BlurDelay,
                blurRevealLength: MimicConstants.BlurLength,
                voteCountManager: (Dictionary<User, (int, double)> usersToVotes) =>
                {
                    CountVotes(usersToVotes, roundTracker);
                },
                votingTime: votingTime)
            {
                VotingTitle = "Find the Original!",
                IndexesOfObjectsToReveal = new List<int>() { indexOfOriginal },
            };
        }
        private void CountVotes(Dictionary<User, (int, double)> usersToVotes, RoundTracker roundTracker)
        {
            foreach (User user in usersToVotes.Keys)
            {
                User userVotedFor = roundTracker.UsersToDisplay[usersToVotes[user].Item1];
                userVotedFor.AddScore(MimicConstants.PointsForVote);

                if (userVotedFor == roundTracker.originalDrawer)
                {
                    user.AddScore(CommonHelpers.PointsForSpeed(
                                maxPoints: MimicConstants.PointsForCorrectPick(this.Lobby.GetAllUsers().Count),
                                minPoints: MimicConstants.PointsForCorrectPick(this.Lobby.GetAllUsers().Count) / 10,
                                startTime: MimicConstants.BlurDelay,
                                endTime: MimicConstants.BlurDelay + MimicConstants.BlurLength,
                                secondsTaken: usersToVotes[user].Item2));
                }
            }
        }
        public void ValidateOptions(List<ConfigureLobbyRequest.GameModeOptionRequest> gameModeOptions)
        {
            //Empty
        }
    } 
}
