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
using RoystonGame.Web.DataModels.Responses;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.TV.GameModes.Common.ThreePartPeople;
using RoystonGame.TV.DataModels.Enums;

namespace RoystonGame.TV.GameModes.BriansGames.BattleReady
{
    public class BattleReadyGameMode : IGameMode
    {
        private ConcurrentBag<PeopleUserDrawing> Drawings { get; set; } = new ConcurrentBag<PeopleUserDrawing>();
        private Lobby Lobby { get; set; }
        private List<Prompt> Prompts { get;} = new List<Prompt>();
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
            int numDrawingsPerPerson = (int)gameModeOptions[(int)GameModeOptionsEnum.numToDraw].ValueParsed;
            int gameSpeed = (int)gameModeOptions[(int)GameModeOptionsEnum.gameSpeed].ValueParsed;
            TimeSpan? setupDrawingTimer = null;
            TimeSpan? setupPromptTimer = null;
            TimeSpan? creationTimer = null;
            TimeSpan? votingTimer = null;
            if (gameSpeed > 0)
            {
                setupDrawingTimer = CommonHelpers.GetTimerFromSpeed(
                    speed: (double)gameSpeed,
                    minTimerLength: BattleReadyConstants.SetupDrawingTimerMin,
                    aveTimerLength: BattleReadyConstants.SetupDrawingTimerAve,
                    maxTimerLength: BattleReadyConstants.SetupDrawingTimerMax);
                setupPromptTimer = CommonHelpers.GetTimerFromSpeed(
                    speed: (double)gameSpeed,
                    minTimerLength: BattleReadyConstants.SetupPromptTimerMin,
                    aveTimerLength: BattleReadyConstants.SetupPromptTimerAve,
                    maxTimerLength: BattleReadyConstants.SetupPromptTimerMax);
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

            int expectedDrawingsPerUser = numDrawingsPerPerson;
            int minDrawingsRequired = numOfEachPartInHand * 3; // the amount to make one playerHand to give everyone

            int expectedPromptsPerUser = numPromptsEachRound * numRounds / lobby.GetAllUsers().Count;
            int minPromptsRequired = numPromptsEachRound * numRounds; // the exact amount of prompts needed for the game

            StateChain setupDrawing = new StateChain(
                stateGenerator: (int counter) =>
                {
                    if (counter == 0)
                    {
                        return GetSetupDrawingGameState(expectedDrawingsPerUser);
                    }
                    else if (counter == 1) // todo figure out some way to do the different body parts all in one state
                    {
                        List<PeopleUserDrawing> drawnHeads = this.Drawings.Where(drawing => drawing.Type == DrawingType.Head).ToList();
                        return GetExtraSetupDrawingGameState(DrawingType.Head, minDrawingsRequired - drawnHeads.Count());
                    }
                    else if (counter == 2) // todo figure out some way to do the different body parts all in one state
                    {
                        List<PeopleUserDrawing> drawnBodies = this.Drawings.Where(drawing => drawing.Type == DrawingType.Body).ToList();
                        return GetExtraSetupDrawingGameState(DrawingType.Head, minDrawingsRequired - drawnBodies.Count());
                    }
                    else if (counter == 3) // todo figure out some way to do the different body parts all in one state
                    {
                        List<PeopleUserDrawing> drawnLegs = this.Drawings.Where(drawing => drawing.Type == DrawingType.Legs).ToList();
                        return GetExtraSetupDrawingGameState(DrawingType.Head, minDrawingsRequired - drawnLegs.Count());
                    }
                    else
                    {
                        return null;
                    }
                });

            StateChain setupPrompt = new StateChain(
                stateGenerator: (int counter) =>
                {
                    if (counter == 0)
                    {
                        return GetSetupPromptGameState(expectedPromptsPerUser);
                    }
                    else if (counter == 1)
                    {
                        if (promptTuples.Count < minPromptsRequired)
                        {
                            return GetExtraSetupPromptGameState(promptTuples, minDrawingsRequired - promptTuples.Count);
                        }
                        return null;
                    }
                    else
                    {
                        return null;
                    }
                });
            setupDrawing.AddExitListener(() =>
            {
                List<(User, PeopleUserDrawing)> drawingTuples = this.Drawings.Select(drawing => (drawing.Owner, drawing)).ToList();
                List<(User, PeopleUserDrawing)> trimmedDrawings = CommonHelpers.TrimUserInputList<PeopleUserDrawing>(drawingTuples, expectedDrawingsPerUser * lobby.GetAllUsers().Count);
                this.Drawings = (ConcurrentBag<PeopleUserDrawing>)trimmedDrawings.Select(drawingTuple => drawingTuple.Item2);
            });
            setupPrompt.AddExitListener(() =>
            {
                List<(User, string)> trimmedPrompts = CommonHelpers.TrimUserInputList<string>(promptTuples.ToList(), expectedPromptsPerUser * lobby.GetAllUsers().Count);
                foreach ((User, string) promptTuple in trimmedPrompts)
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

            #region GameState Generators
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
            GameState GetSetupDrawingGameState(int numExpectedPerUser)
            {
                List<DrawingType> randomizedDrawingTypes = new List<DrawingType>() { DrawingType.Head, DrawingType.Body, DrawingType.Legs };
                return new SetupGameState(
                    lobby: lobby,
                    countingPromptGenerator: (User user, int counter) =>
                    {
                        if (counter % 3 == 0)
                        {
                            randomizedDrawingTypes = randomizedDrawingTypes.OrderBy(_ => Rand.Next()).ToList();
                        }
                        DrawingType drawingType = randomizedDrawingTypes[counter % 3];
                        return new UserPrompt()
                        {
                            Title = "Time to draw!",
                            Description = "Draw the prompt below. Keep in mind you are only drawing part of the person!",
                            SubPrompts = new SubPrompt[]
                            {
                                new SubPrompt
                                {
                                    Prompt = Invariant($"Draw any \"{drawingType.ToString()}\""),
                                    Drawing = new DrawingPromptMetadata()
                                    {
                                        WidthInPx = ThreePartPeopleConstants.Widths[drawingType],
                                        HeightInPx = ThreePartPeopleConstants.Heights[drawingType],
                                        CanvasBackground = ThreePartPeopleConstants.Backgrounds[drawingType],
                                    },
                                },
                            },
                            SubmitButton = true
                        };
                    },
                    countingFormSubmitHandler: (User user, UserFormSubmission input, int counter) =>
                    {
                        this.Drawings.Add(new PeopleUserDrawing
                        {
                            Drawing = input.SubForms[0].Drawing,
                            Owner = user,
                            Type = randomizedDrawingTypes[counter % 3]
                        });
                        return (true, string.Empty);
                    },
                    countingUserTimeoutHandler: (User user, UserFormSubmission input, int counter) =>
                    {
                        if (input?.SubForms?[0]?.Drawing != null)
                        {
                            Drawings.Add(new PeopleUserDrawing
                            {
                                Drawing = input.SubForms[0].Drawing,
                                Owner = user,
                                Type = randomizedDrawingTypes[counter % 3]
                            });
                        }
                        return UserTimeoutAction.None;
                    },
                    perUserExpectedAmount: numExpectedPerUser,
                    unityTitle: "",
                    unityInstructions: "Complete as many drawings as possible before the time runs out",
                    setupDurration: setupDrawingTimer);
            }
            GameState GetSetupPromptGameState(int numExpectedPerUser)
            {
                return new SetupGameState(
                    lobby: lobby,
                    countingPromptGenerator: (User user, int counter) =>
                    {
                        return new UserPrompt()
                        {
                            Title = "Now lets make some battle prompts!",
                            Description = "Examples: Who would win in a fight, Who would make the best actor, Etc.",
                            SubPrompts = new SubPrompt[]
                            {
                                new SubPrompt
                                {
                                    Prompt="Prompt",
                                    ShortAnswer=true
                                },
                            },
                            SubmitButton = true
                        };
                    },
                    countingFormSubmitHandler: (User user, UserFormSubmission input, int counter) =>
                    {
                        if (promptTuples.Select((tuple) => tuple.Item2).Contains(input.SubForms[0].ShortAnswer))
                        {
                            return (false, "Someone has already entered that prompt");
                        }
                        promptTuples.Add((user, input.SubForms[0].ShortAnswer));
                        return (true, String.Empty);
                    },
                    countingUserTimeoutHandler: (User user, UserFormSubmission input, int counter) =>
                    {
                        if (input?.SubForms?[0]?.ShortAnswer != null
                        && !promptTuples.Select((tuple) => tuple.Item2).Contains(input.SubForms[0].ShortAnswer))
                        {
                            promptTuples.Add((user, input.SubForms[0].ShortAnswer));
                        }
                        return UserTimeoutAction.None;
                    },
                    perUserExpectedAmount: numExpectedPerUser,
                    unityTitle: "",
                    unityInstructions: "Complete as many prompts as possible before the time runs out",
                    setupDurration: setupPromptTimer);
            }
            GameState GetExtraSetupDrawingGameState(DrawingType drawingType, int numNeeded)
            {
                return new ExtraSetupGameState(
                    lobby: lobby,
                    promptGenerator: (User user) => new UserPrompt()
                    {
                        Title = "Time to draw!",
                        Description = "Draw the prompt below. Keep in mind you are only drawing part of the person!",
                        SubPrompts = new SubPrompt[]
                        {
                            new SubPrompt
                            {
                                Prompt = Invariant($"Draw any \"{drawingType.ToString()}\""),
                                Drawing = new DrawingPromptMetadata()
                                {
                                    WidthInPx = ThreePartPeopleConstants.Widths[drawingType],
                                    HeightInPx = ThreePartPeopleConstants.Heights[drawingType],
                                    CanvasBackground = ThreePartPeopleConstants.Backgrounds[drawingType],
                                },
                            },
                        },
                        SubmitButton = true
                    },
                    formSubmitHandler: (User user, UserFormSubmission input) =>
                    {
                        this.Drawings.Add(new PeopleUserDrawing
                        {
                            Drawing = input.SubForms[0].Drawing,
                            Owner = user,
                            Type = drawingType
                        });
                        return (true, string.Empty);
                    },
                    numExtraObjectsNeeded: numNeeded);
            }
            GameState GetExtraSetupPromptGameState (ConcurrentBag<(User, string)> promptTuples, int numNeeded)
            {
                return new ExtraSetupGameState(
                    lobby: lobby,
                    promptGenerator: (User user) => new UserPrompt()
                    {
                        Title = "Now lets make some battle prompts!",
                        Description = "Examples: Who would win in a fight, Who would make the best actor, Etc.",
                        SubPrompts = new SubPrompt[]
                        {
                            new SubPrompt
                            {
                                Prompt="Prompt",
                                ShortAnswer=true
                            },
                        },
                        SubmitButton = true
                    },
                    formSubmitHandler: (User user, UserFormSubmission input) =>
                    {
                        if (promptTuples.Select((tuple) => tuple.Item2).Contains(input.SubForms[0].ShortAnswer))
                        {
                            return (false, "Someone has already entered that prompt");
                        }
                        promptTuples.Add((user, input.SubForms[0].ShortAnswer));
                        return (true, String.Empty);
                    },
                    numExtraObjectsNeeded: numNeeded);
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
            #endregion

            this.Entrance.Transition(setupDrawing);
            setupDrawing.Transition(setupPrompt);
            setupPrompt.Transition(CreateContestantCreationGamestate);
           
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
