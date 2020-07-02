using RoystonGame.TV.DataModels.States.GameStates;
using RoystonGame.TV.Extensions;
using RoystonGame.TV.GameModes.KevinsGames.Mimic.GameStates;
using RoystonGame.Web.DataModels.Requests.LobbyManagement;
using System.Collections.Generic;
using RoystonGame.Web.DataModels.Exceptions;
using RoystonGame.TV.GameModes.Common.GameStates;
using RoystonGame.TV.GameModes.BriansGames.Common.GameStates;
using static RoystonGame.TV.GameModes.Common.ThreePartPeople.DataModels.Person;
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

namespace RoystonGame.TV.GameModes.KevinsGames.Mimic
{
    public class MimicGameMode : IGameMode
    {
        private ConcurrentBag<UserDrawing> Drawings { get; set; } = new ConcurrentBag<UserDrawing>();
        private GameState Setup { get; set; }
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
                                        displayTimeDurration: TimeSpan.FromSeconds(memorizeTimerLength),
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
                                else if (counter >= maxDrawingsBeforeVote && counter - maxDrawingsBeforeVote < maxDrawingsBeforeVote * 2)
                                {
                                    if ((counter - maxDrawingsBeforeVote) % 2 == 0)
                                    {
                                        RoundTracker votingRoundTracker = roundTrackers[(counter - maxDrawingsBeforeVote) / 2];

                                        return new Voting_GS(
                                            lobby: lobby,
                                            roundTracker: votingRoundTracker,
                                            votingTime: TimeSpan.FromSeconds(votingTimerLength),
                                            gameModeOptions: gameModeOptions);
                                    }

                                    else
                                    {
                                        RoundTracker votingRoundTracker = roundTrackers[(counter - maxDrawingsBeforeVote) / 2];
                                        return new VoteRevealed_GS(
                                            lobby: lobby,
                                            roundTracker: votingRoundTracker);
                                    }

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
        public void ValidateOptions(List<ConfigureLobbyRequest.GameModeOptionRequest> gameModeOptions)
        {
            //Empty
        }
    } 
}
