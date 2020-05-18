using RoystonGame.TV.DataModels.States.GameStates;
using RoystonGame.TV.Extensions;
using RoystonGame.TV.GameModes.BriansGames.BattleReady.DataModels;
using RoystonGame.TV.GameModes.BriansGames.BattleReady.GameStates;
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

namespace RoystonGame.TV.GameModes.BriansGames.BattleReady
{
    public class BattleReadyGameMode : IGameMode
    {
        private List<PeopleUserDrawing> Drawings { get; set; } = new List<PeopleUserDrawing>();
        private List<Prompt> Prompts { get;} = new List<Prompt>();
        private GameState Setup { get; set; }
        private RoundTracker RoundTracker { get; } = new RoundTracker();
        private Random Rand { get; } = new Random();
        public BattleReadyGameMode(Lobby lobby, List<ConfigureLobbyRequest.GameModeOptionRequest> gameModeOptions)
        {
            ValidateOptions(gameModeOptions);

            List<(User, string)> promptTuples = new List<(User, string)>();
            List<Prompt> promptsCopy = new List<Prompt>();
            int numRounds = int.Parse(gameModeOptions[0].ShortAnswer);
            int numPromptForEachUsersPerRound = int.Parse(gameModeOptions[1].ShortAnswer);
            int numDrawingsPerPersonPerPart = int.Parse(gameModeOptions[2].ShortAnswer);
            int numOfEachPartInHand = 3;
            int numUsersPerPrompt = 2;

            int numPromptsEachRound = numPromptForEachUsersPerRound * lobby.GetAllUsers().Count / numUsersPerPrompt;
            int numPromptsNeededFromUser = numRounds * numPromptForEachUsersPerRound / numUsersPerPrompt;

            Setup = new Setup_GS(
                lobby: lobby, 
                drawings: Drawings,
                prompts: promptTuples,
                numDrawingsPerUserPerPart: numDrawingsPerPersonPerPart,
                numPromptsPerUser: numPromptsNeededFromUser);

            Setup.AddExitListener(() =>
            {
                List<PeopleUserDrawing> randomizedHeads = Drawings.FindAll((drawing) => drawing.Type == DrawingType.Head).OrderBy(_ => Rand.Next()).ToList();
                List<PeopleUserDrawing> randomizedBodies = Drawings.FindAll((drawing) => drawing.Type == DrawingType.Body).OrderBy(_ => Rand.Next()).ToList();
                List<PeopleUserDrawing> randomizedLegs = Drawings.FindAll((drawing) => drawing.Type == DrawingType.Legs).OrderBy(_ => Rand.Next()).ToList();
                int totalDrawings = lobby.GetAllUsers().Count* numPromptForEachUsersPerRound* 3*numOfEachPartInHand;
                while (Drawings.Count < totalDrawings)
                {
                    if(randomizedHeads.Count*randomizedBodies.Count*randomizedLegs.Count == 0)
                    {
                        randomizedHeads = Drawings.FindAll((drawing) => drawing.Type == DrawingType.Head).OrderBy(_ => Rand.Next()).ToList();
                        randomizedBodies = Drawings.FindAll((drawing) => drawing.Type == DrawingType.Body).OrderBy(_ => Rand.Next()).ToList();
                        randomizedLegs = Drawings.FindAll((drawing) => drawing.Type == DrawingType.Legs).OrderBy(_ => Rand.Next()).ToList();
                    }
                    Drawings.Add(randomizedHeads[0]);
                    randomizedHeads.RemoveAt(0);

                    Drawings.Add(randomizedBodies[0]);
                    randomizedBodies.RemoveAt(0);

                    Drawings.Add(randomizedLegs[0]);
                    randomizedLegs.RemoveAt(0);
                }
                Drawings = Drawings.OrderBy(_ => Rand.Next()).ToList();
                foreach ((User, string) promptTuple in promptTuples)
                {
                    Prompts.Add(new Prompt
                    {
                        Owner = promptTuple.Item1,
                        Text = promptTuple.Item2
                    });
                }
                promptsCopy = Prompts.ToList();
            });

            
            List<GameState> creationGameStates = new List<GameState>();
            List<GameState> votingGameStates = new List<GameState>();
            List<GameState> voteRevealedGameStates = new List<GameState>();
            List<GameState> scoreboardGameStates = new List<GameState>();

            int countRounds = 0;
            GameState CreateContestantCreationGamestate()
            {
                RoundTracker.ResetRoundVariables();
                promptsCopy = promptsCopy.OrderBy(_ => Rand.Next()).ToList();
                List<Prompt> roundPrompts = promptsCopy.GetRange(0, numPromptsEachRound);
                promptsCopy.RemoveRange(0, numPromptsEachRound);

                List<PeopleUserDrawing> randomizedHeads = Drawings.FindAll((drawing) => drawing.Type == DrawingType.Head).OrderBy(_ => Rand.Next()).ToList();
                List<PeopleUserDrawing> randomizedBodies = Drawings.FindAll((drawing) => drawing.Type == DrawingType.Body).OrderBy(_ => Rand.Next()).ToList();
                List<PeopleUserDrawing> randomizedLegs = Drawings.FindAll((drawing) => drawing.Type == DrawingType.Legs).OrderBy(_ => Rand.Next()).ToList();
                
                for (int i = 0; i < numPromptForEachUsersPerRound; i++)
                {
                    foreach (User user in lobby.GetAllUsers())
                    {
                        List<Prompt> roundPromptsCopy = roundPrompts.ToList();
                        Prompt randPrompt = null;
                        foreach (Prompt prompt in roundPrompts.OrderBy(_ => Rand.Next()).ToList())
                        {
                            if(!prompt.UsersToUserHands.Keys.Contains(user) && prompt.UsersToUserHands.Keys.Count() < numUsersPerPrompt)
                            {
                                randPrompt = prompt;
                                break;
                            }
                        }
                        if(randPrompt == null)
                        {
                            throw new Exception("Something went wrong while setting up the game");
                        }
                        randPrompt.UsersToUserHands.TryAdd(user, new Prompt.UserHand
                        {
                            Heads = randomizedHeads.GetRange(0, numOfEachPartInHand),
                            Bodies = randomizedBodies.GetRange(0, numOfEachPartInHand),
                            Legs = randomizedLegs.GetRange(0, numOfEachPartInHand),
                            Contestant = new Person()
                        });
                        randomizedHeads.RemoveRange(0, numOfEachPartInHand);
                        randomizedBodies.RemoveRange(0, numOfEachPartInHand);
                        randomizedLegs.RemoveRange(0, numOfEachPartInHand);
                        roundPromptsCopy.Remove(randPrompt);
                        if (!RoundTracker.UsersToAssignedPrompts.ContainsKey(user))
                        {
                            RoundTracker.UsersToAssignedPrompts.Add(user, new List<Prompt>());
                        }
                        RoundTracker.UsersToAssignedPrompts[user].Add(randPrompt);
                    }

                }
                GameState toReturn = new ContestantCreation_GS(
                        lobby: lobby,
                        roundTracker: RoundTracker);
                toReturn.Transition(CreateVotingGameStates(roundPrompts));
                return toReturn;
            }
            Func<StateChain> CreateVotingGameStates(List<Prompt> roundPrompts)
            {
                return () =>
                {
                    StateChain voting = new StateChain(
                        stateGenerator: (int counter) =>
                        {
                            int round = counter / 2;
                            int type = counter % 2; // Even rounds are Voting, Odd rounds are reveals
                            if (round < roundPrompts.Count)
                            {
                                if (type == 0)
                                {
                                    return new Voting_GS(
                                        lobby: lobby,
                                        prompt: roundPrompts[round]);
                                }
                                else
                                {
                                    return new VoteRevealed_GS(
                                        lobby: lobby,
                                        prompt: roundPrompts[round]);
                                }
                            }
                            else
                            {
                                // Stops the chain.
                                return null;
                            }
                        });
                    voting.Transition(CreateScoreGameState(roundPrompts));
                    return voting;
                };
            }
            
            Func<GameState> CreateScoreGameState(List<Prompt> roundPrompts)
            {
                return () =>
                {
                    List<Person> winnersPeople = roundPrompts.Select((prompt) => prompt.UsersToUserHands[prompt.Winner].Contestant).ToList();
                    
                    countRounds++;
                    GameState displayPeople = new DisplayPeople_GS(
                        lobby: lobby,
                        title: "Here are your winners",
                        peopleList: winnersPeople,
                        imageTitle: (person) => roundPrompts[winnersPeople.IndexOf(person)].Text,
                        imageHeader: (person) => person.Name
                        );

                    if (countRounds >= numRounds)
                    {
                        GameState finalScoreBoard = new ScoreBoardGameState(
                        lobby: lobby,
                        title: "Final Scores");
                        displayPeople.Transition(finalScoreBoard);
                        finalScoreBoard.Transition(this.Exit);
                    }
                    else
                    {
                        GameState scoreBoard = new ScoreBoardGameState(
                            lobby: lobby);
                        displayPeople.Transition(scoreBoard);
                        scoreBoard.Transition(CreateContestantCreationGamestate);
                    }
                    return displayPeople;
                };
            }
            Setup.Transition(CreateContestantCreationGamestate);
            this.Entrance.Transition(Setup);
           
        }

        public void ValidateOptions(List<ConfigureLobbyRequest.GameModeOptionRequest> gameModeOptions)
        {

            if (!int.TryParse(gameModeOptions[0].ShortAnswer, out int parsedInteger1)
                || parsedInteger1 < 1 || parsedInteger1 > 30)
            {
                throw new GameModeInstantiationException("Numer of rounds must be an integer from 1-30");
            }
            if (!int.TryParse(gameModeOptions[1].ShortAnswer, out int parsedInteger2)
               || parsedInteger2 < 1 || parsedInteger2 > 30)
            {
                throw new GameModeInstantiationException("Numer of prompts per round must be an integer from 1-30");
            }
            /*if(gameLobby.GetAllUsers().Count%2==1 && int.Parse(gameModeOptions[1].ShortAnswer)%2==1)
            {
                throw new GameModeInstantiationException("Numer of prompts per round must even if you have an odd number of players");
            }*/
            if (!int.TryParse(gameModeOptions[2].ShortAnswer, out int parsedInteger3)
                || parsedInteger3 < 1 || parsedInteger3 > 30)
            {
                throw new GameModeInstantiationException(Invariant($"Numer of drawings per round must be an integer from 1-30"));
            }
        }
    }
}
