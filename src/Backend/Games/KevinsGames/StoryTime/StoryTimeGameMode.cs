using Backend.GameInfrastructure.DataModels.States.GameStates;
using Backend.GameInfrastructure.Extensions;
using Backend.Games.KevinsGames.StoryTime.GameStates;
using Common.DataModels.Requests.LobbyManagement;
using System.Collections.Generic;
using Backend.Games.Common.GameStates;
using Backend.GameInfrastructure.DataModels.Users;
using System;
using System.Linq;
using Backend.GameInfrastructure.DataModels.States.StateGroups;
using Backend.Games.KevinsGames.StoryTime.DataModels;
using Backend.GameInfrastructure;

namespace Backend.Games.KevinsGames.StoryTime
{
    public class StoryTimeGameMode : IGameMode
    {
        private Random Rand { get; } = new Random();
        private GameState Setup { get; set;}
        public StoryTimeGameMode(Lobby lobby, List<ConfigureLobbyRequest.GameModeOptionRequest> gameModeOptions)
        {
            ValidateOptions(gameModeOptions);
            int numPlayersPerRound = (int)gameModeOptions[0].ValueParsed;
            int numRounds = (int)gameModeOptions[1].ValueParsed;
            int writingTimerLength = (int)gameModeOptions[2].ValueParsed;
            int votingTimerLength = (int)gameModeOptions[3].ValueParsed;
            int characterLimit = (int)gameModeOptions[4].ValueParsed;

            string oldText = "Error: Old Text Not Set Please Show This To Brian";

            string setupPrompt = StoryTimeConstants.WritingPrompts[Rand.Next(0, StoryTimeConstants.WritingPrompts.Count - 1)];
            RoundTracker setupTracker = new RoundTracker();
            Setup = new Setup_GS(
                lobby: lobby,
                prompt: setupPrompt,
                roundTracker: setupTracker,
                writingDuration: TimeSpan.FromSeconds(writingTimerLength));
            
            StateChain CreateSetupVoting()
            {
                StateChain setupVoting = new StateChain(stateGenerator: (int counter) =>
                {
                    if(setupTracker.UsersToUserWriting.Count <= 0)
                    {
                        oldText = "Nobody finished their starting sentences.";
                        return null;                        
                    }
                    if (counter == 0)
                    {
                        return new Voting_GS(
                            lobby: lobby,
                            oldText: "",
                            prompt: setupPrompt,
                            roundTracker: setupTracker);
                    }
                    else if (counter ==1)
                    {
                        oldText = setupTracker.Winner.Text;
                        return new VoteRevealed_GS(
                            lobby: lobby,
                            roundTracker: setupTracker,
                            oldText: "",
                            prompt: setupPrompt);
                    }
                    else
                    {
                        return null;
                    }
                });
                setupVoting.Transition(CreateGamePlayLoop);
                return setupVoting;
            }

            bool timeForScores = true;
            StateChain CreateGamePlayLoop()
            {
                StateChain gamePlayLoop = new StateChain(stateGenerator: (int counter) =>
                {
                    if (counter < numRounds)
                    {
                        RoundTracker roundTracker = new RoundTracker();
                        string prompt = StoryTimeConstants.WritingPrompts[Rand.Next(0, StoryTimeConstants.WritingPrompts.Count - 1)];
                        List<User> usersToWrite = lobby.GetAllUsers().OrderBy(_ => Rand.Next()).ToList().GetRange(0, numPlayersPerRound); // fix to make it evenly distribute instead of random
                        return new StateChain(stateGenerator: (int counter) =>
                        {
                            if (counter == 0)
                            {
                                return new Writing_GS(
                                    lobby: lobby,
                                    usersWriting: usersToWrite,
                                    prompt: prompt,
                                    oldText: oldText,
                                    roundTracker: roundTracker,
                                    writingDuration: TimeSpan.FromSeconds(writingTimerLength));
                            }
                            else if (counter == 1)
                            {
                                if(roundTracker.UsersToUserWriting.Count <= 0)
                                {
                                    // no users finished their input and so skip the voting and just move to the next round
                                    return null;
                                }
                                return new Voting_GS(
                                    lobby: lobby,
                                    oldText: oldText,
                                    prompt: prompt,
                                    roundTracker: roundTracker);
                            }
                            else if (counter ==2)
                            {
                                string tempOldText = oldText;
                                oldText = roundTracker.Winner.Text;
                                return new VoteRevealed_GS(
                                    lobby: lobby,
                                    roundTracker: roundTracker,
                                    oldText: tempOldText,
                                    prompt: prompt);
                            }
                            else
                            {
                                //breaks chain
                                return null;
                            }
                        });
                    }
                    else
                    {
                        if(timeForScores)
                        {
                            timeForScores = false;
                            return new ScoreBoardGameState(
                                lobby: lobby,
                                title: "Final Scores");
                        }
                        else
                        {
                            //breaks cycle
                            return null;
                        }       
                    }
                });
                gamePlayLoop.Transition(this.Exit);
                return gamePlayLoop;
            }
            this.Entrance.Transition(Setup);
            Setup.Transition(CreateSetupVoting);
            
        }
        public void ValidateOptions(List<ConfigureLobbyRequest.GameModeOptionRequest> gameModeOptions)
        {
            //Empty
        }
    }
}
