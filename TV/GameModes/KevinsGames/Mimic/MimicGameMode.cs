using RoystonGame.TV.DataModels.States.GameStates;
using RoystonGame.TV.Extensions;
using RoystonGame.TV.GameModes.KevinsGames.Mimic.GameStates;
using RoystonGame.Web.DataModels.Requests.LobbyManagement;
using System.Collections.Generic;
using RoystonGame.Web.DataModels.Exceptions;
using RoystonGame.TV.GameModes.Common.GameStates;
using static System.FormattableString;
using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.GameModes.Common.ThreePartPeople.DataModels;
using System;
using System.Linq;
using RoystonGame.TV.DataModels.States.StateGroups;
using System.Collections.Concurrent;
using RoystonGame.TV.GameModes.Common.DataModels;
using RoystonGame.TV.GameModes.KevinsGames.Mimic.DataModels;
using Microsoft.AspNetCore.Mvc;
using RoystonGame.TV.DataModels.States.UserStates;
using RoystonGame.TV.DataModels;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Server.IIS.Core;
using Microsoft.Identity.Client;
using RoystonGame.TV.GameModes.Common.GameStates.VoteAndReveal;
using RoystonGame.TV.GameModes.Common;

namespace RoystonGame.TV.GameModes.KevinsGames.Mimic
{
    public class MimicGameMode : IGameMode
    {
        private ConcurrentBag<UserDrawing> Drawings { get; set; } = new ConcurrentBag<UserDrawing>();
        private GameState Setup { get; set; }
        private Lobby Lobby { get; set; }
        private Random Rand { get; } = new Random();
        public MimicGameMode(Lobby lobby, List<ConfigureLobbyRequest.GameModeOptionRequest> gameModeOptions)
        {
            ValidateOptions(gameModeOptions);
            int numStartingDrawingsPerUser = (int)gameModeOptions[(int)GameModeOptions.NumStartingDrawingsPerUser].ValueParsed;
            int memorizeTimerLength = (int)gameModeOptions[(int)GameModeOptions.MemorizeTimerLength].ValueParsed;
            int drawingTimerLength = (int)gameModeOptions[(int)GameModeOptions.DrawingTimerLength].ValueParsed;
            int votingTimerLength = (int)gameModeOptions[(int)GameModeOptions.VotingTimerLength].ValueParsed;
            int maxDrawingsBeforeVoteInput = (int)gameModeOptions[(int)GameModeOptions.MaxDrawingsBeforeVote].ValueParsed;
            int numSets = (int)gameModeOptions[(int)GameModeOptions.NumSets].ValueParsed;
            int maxVoteDrawings = (int)gameModeOptions[(int)GameModeOptions.MaxVoteDrawings].ValueParsed;

            this.Lobby = lobby;

            Setup = new Setup_GS(
                lobby: lobby,
                drawings: Drawings,
                numDrawingsPerUser: numStartingDrawingsPerUser,
                drawingTimeDurration: TimeSpan.FromSeconds(drawingTimerLength));
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
                                        displayTimeDuration: TimeSpan.FromSeconds(memorizeTimerLength),
                                        displayDrawing: originalDrawing);
                                    CreateMimics_GS mimicsGS = new CreateMimics_GS(
                                        lobby: lobby,
                                        roundTracker: roundTrackers.Last(),
                                        drawingTimeDurration: TimeSpan.FromSeconds(drawingTimerLength * MimicConstants.MimicTimerMultiplier)
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
                                    return GetVotingAndRevealState(roundTrackers[counter - maxDrawingsBeforeVote], TimeSpan.FromSeconds(votingTimerLength));
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
                indexesOfDrawingsToReveal: new List<int>() { indexOfOriginal },
                votingTime: votingTime)
            {
                VotingTitle = "Find the Original!",
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
