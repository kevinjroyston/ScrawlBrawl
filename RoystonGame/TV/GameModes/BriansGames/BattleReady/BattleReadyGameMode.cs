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
using System.Collections.Concurrent;
using RoystonGame.TV.GameModes.Common.DataModels;
using RoystonGame.TV.GameModes.Common.GameStates.VoteAndReveal;
using RoystonGame.TV.DataModels;
using RoystonGame.TV.GameModes.Common;

namespace RoystonGame.TV.GameModes.BriansGames.BattleReady
{
    public class BattleReadyGameMode : IGameMode
    {
        private ConcurrentBag<PeopleUserDrawing> Drawings { get; set; } = new ConcurrentBag<PeopleUserDrawing>();
        private Lobby Lobby { get; set; }
        private List<Prompt> Prompts { get;} = new List<Prompt>();
        private GameState Setup { get; set; }
        private RoundTracker RoundTracker { get; } = new RoundTracker();
        private Random Rand { get; } = new Random();
        public BattleReadyGameMode(Lobby lobby, List<ConfigureLobbyRequest.GameModeOptionRequest> gameModeOptions)
        {
            ValidateOptions(gameModeOptions);

            this.Lobby = lobby;
            ConcurrentBag<(User, string)> promptTuples = new ConcurrentBag<(User, string)>();
            List<Prompt> promptsCopy = new List<Prompt>();
            int numRounds = (int)gameModeOptions[(int)GameModeOptionsEnum.numRounds].ValueParsed;
            int numPromptsForEachUserPerRound = (int)gameModeOptions[(int)GameModeOptionsEnum.numPrompts].ValueParsed;
            int numDrawingsPerPersonPerPart = (int)gameModeOptions[(int)GameModeOptionsEnum.numToDraw].ValueParsed;
            int gameSpeed = (int)gameModeOptions[(int)GameModeOptionsEnum.gameSpeed].ValueParsed;
            TimeSpan? setupTimer = null;
            TimeSpan? creationTimer = null;
            TimeSpan? votingTimer = null;
            if (gameSpeed > 0)
            {
                setupTimer = CommonHelpers.GetTimerFromSpeed(
                    speed: (double)gameSpeed,
                    minTimerLength: BattleReadyConstants.SetupTimerMin,
                    aveTimerLength: BattleReadyConstants.SetupTimerAve,
                    maxTimerLength: BattleReadyConstants.SetupTimerMax);
                creationTimer = CommonHelpers.GetTimerFromSpeed(
                    speed: (double)gameSpeed,
                    minTimerLength: BattleReadyConstants.CreationTimerMin,
                    aveTimerLength: BattleReadyConstants.CreationTimerAve,
                    maxTimerLength: BattleReadyConstants.CreationTimerMax);
                votingTimer = CommonHelpers.GetTimerFromSpeed(
                    speed: (double)gameSpeed,
                    minTimerLength: BattleReadyConstants.VotingTimerMin,
                    aveTimerLength: BattleReadyConstants.VotingTimerAve,
                    maxTimerLength: BattleReadyConstants.VotingTimerMax);
            }

            int numOfEachPartInHand = 3;
            int numUsersPerPrompt = 2;

            int numPromptsEachRound = numPromptsForEachUserPerRound * lobby.GetAllUsers().Count / numUsersPerPrompt;
            int numPromptsNeededFromUser = numRounds * numPromptsForEachUserPerRound / numUsersPerPrompt;

            Setup = new Setup_GS(
                lobby: lobby, 
                drawings: Drawings,
                prompts: promptTuples,
                numDrawingsPerUserPerPart: numDrawingsPerPersonPerPart,
                numPromptsPerUser: numPromptsNeededFromUser,
                setupDuration: setupTimer);

            Setup.AddExitListener(() =>
            {
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

                List<PeopleUserDrawing> headDrawings = Drawings.ToList().FindAll((drawing) => drawing.Type == DrawingType.Head).OrderBy(_ => Rand.Next()).ToList();
                List<PeopleUserDrawing> bodyDrawings = Drawings.ToList().FindAll((drawing) => drawing.Type == DrawingType.Body).OrderBy(_ => Rand.Next()).ToList();
                List<PeopleUserDrawing> legsDrawings = Drawings.ToList().FindAll((drawing) => drawing.Type == DrawingType.Legs).OrderBy(_ => Rand.Next()).ToList();
                List<PeopleUserDrawing> headDrawingsCopy = headDrawings.ToList();
                List<PeopleUserDrawing> bodyDrawingsCopy = bodyDrawings.ToList();
                List<PeopleUserDrawing> legsDrawingsCopy = legsDrawings.ToList();
                if(headDrawings.Count != bodyDrawings.Count || bodyDrawings.Count != legsDrawings.Count)
                {
                    throw new Exception("Something went wrong while setting up the game");
                }

                for (int i = 0; i < numPromptsForEachUserPerRound; i++)
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

                        List<PeopleUserDrawing> headDrawingsToAdd = new List<PeopleUserDrawing>();
                        List<PeopleUserDrawing> bodyDrawingsToAdd = new List<PeopleUserDrawing>();
                        List<PeopleUserDrawing> legsDrawingsToAdd = new List<PeopleUserDrawing>();    
                        
                        for(int j = 0; j < numOfEachPartInHand; j++)
                        {
                            if(headDrawingsCopy.Count<headDrawings.Count)
                            {
                                headDrawingsCopy.AddRange(headDrawings.OrderBy(_ => Rand.Next()));
                            }
                            if (bodyDrawingsCopy.Count < bodyDrawings.Count)
                            {
                                bodyDrawingsCopy.AddRange(bodyDrawings.OrderBy(_ => Rand.Next()));
                            }
                            if (legsDrawingsCopy.Count < legsDrawings.Count)
                            {
                                legsDrawingsCopy.AddRange(legsDrawings.OrderBy(_ => Rand.Next()));
                            }

                            headDrawingsToAdd.Add(headDrawingsCopy.Find((drawing) => !headDrawingsToAdd.Contains(drawing)));
                            headDrawingsCopy.Remove(headDrawingsToAdd.Last());

                            bodyDrawingsToAdd.Add(bodyDrawingsCopy.Find((drawing) => !bodyDrawingsToAdd.Contains(drawing)));
                            bodyDrawingsCopy.Remove(bodyDrawingsToAdd.Last());

                            legsDrawingsToAdd.Add(legsDrawingsCopy.Find((drawing) => !legsDrawingsToAdd.Contains(drawing)));
                            legsDrawingsCopy.Remove(legsDrawingsToAdd.Last());
                        }
                        randPrompt.UsersToUserHands.TryAdd(user, new Prompt.UserHand
                        {
                            Heads = headDrawingsToAdd,
                            Bodies = bodyDrawingsToAdd,
                            Legs = legsDrawingsToAdd,
                            Contestant = new Person()
                        });
                        
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
                        roundTracker: RoundTracker,
                        creationDuration: creationTimer);
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
                            if (counter < roundPrompts.Count)
                            {
                                Prompt roundPrompt = roundPrompts[counter];

                                return GetVotingAndRevealState(roundPrompt, votingTimer);         
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
            /*if(gameLobby.GetAllUsers().Count%2==1 && int.Parse(gameModeOptions[1].ShortAnswer)%2==1)
            {
                throw new GameModeInstantiationException("Numer of prompts per round must even if you have an odd number of players");
            }*/
            if ((int)gameModeOptions[1].ValueParsed % 2 == 1)
            {
                throw new GameModeInstantiationException("Numer of prompts per round must even");
            }
        }
        private State GetVotingAndRevealState(Prompt prompt, TimeSpan? votingTime)
        {
            List<User> randomizedUsersToDisplay = prompt.UsersToUserHands.Keys.OrderBy(_ => Rand.Next()).ToList();
            List<Person> peopleToVoteOn = randomizedUsersToDisplay.Select(user => prompt.UsersToUserHands[user].Contestant).ToList();
            List<string> imageTitles = randomizedUsersToDisplay.Select(user => prompt.UsersToUserHands[user].Contestant.Name).ToList();

            return new ThreePartPersonVoteAndRevealState(
                lobby: this.Lobby,
                people: peopleToVoteOn,
                voteCountManager: (Dictionary<User, int> usersToVotes) =>
                {
                    CountVotes(usersToVotes, prompt, randomizedUsersToDisplay);
                },
                votingTime: votingTime)
            {
                VotingTitle = prompt.Text,
                ObjectTitles = imageTitles,
                ShowObjectTitlesForVoting = true,
            };
        }
        private void CountVotes(Dictionary<User, int> usersToVotes, Prompt prompt, List<User> answerUsers)
        {

            foreach (User user in usersToVotes.Keys)
            {
                User userVotedFor = answerUsers[usersToVotes[user]];
                userVotedFor.AddScore(BattleReadyConstants.PointsForVote);
                prompt.UsersToUserHands[userVotedFor].VotesForContestant++;
                if (prompt.Winner == null || prompt.UsersToUserHands[userVotedFor].VotesForContestant > prompt.UsersToUserHands[prompt.Winner].VotesForContestant)
                {
                    prompt.Winner = userVotedFor;
                }
            }
        }
    }
}
