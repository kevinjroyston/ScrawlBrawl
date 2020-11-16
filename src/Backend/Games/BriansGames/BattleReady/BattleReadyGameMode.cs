using Backend.GameInfrastructure.DataModels.States.GameStates;
using Backend.GameInfrastructure.Extensions;
using Backend.Games.BriansGames.BattleReady.DataModels;
using Backend.Games.BriansGames.BattleReady.GameStates;
using Common.DataModels.Requests.LobbyManagement;
using System.Collections.Generic;
using Backend.Games.Common.GameStates;
using Backend.Games.BriansGames.Common.GameStates;
using static Backend.Games.Common.ThreePartPeople.DataModels.Person;
using Backend.GameInfrastructure.DataModels.Users;
using Backend.Games.Common.ThreePartPeople.DataModels;
using System;
using System.Linq;
using Backend.GameInfrastructure.DataModels.States.StateGroups;
using System.Collections.Concurrent;
using Backend.Games.Common.GameStates.VoteAndReveal;
using Backend.GameInfrastructure.DataModels;
using Backend.Games.Common;
using Common.DataModels.Responses;
using Microsoft.CodeAnalysis;
using Backend.GameInfrastructure;
using Common.DataModels.Enums;
using Common.Code.Helpers;
using Common.DataModels.Interfaces;

namespace Backend.Games.BriansGames.BattleReady
{
    public class BattleReadyGameMode : IGameMode
    {
        private ConcurrentBag<PeopleUserDrawing> Drawings { get; set; } = new ConcurrentBag<PeopleUserDrawing>();
        private Lobby Lobby { get; set; }
        private ConcurrentBag<Prompt> Prompts { get; set; } = new ConcurrentBag<Prompt>();
        private RoundTracker RoundTracker { get; } = new RoundTracker();
        private Random Rand { get; } = new Random();

        public static GameModeMetadata GameModeMetadata { get; } = new GameModeMetadata
        {
            Title = "Body Builder", // in code refered to as Battle Ready
            GameId = GameModeId.BodyBuilder,
            Description = "Go head to head body to body and legs to legs with other players to try to make the best constestant for each challenge.",
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
                        Description = "Number of rounds",
                        ResponseType = ResponseType.Integer,
                        DefaultValue = 3,
                        MinValue = 1,
                        MaxValue = 30,
                    },
                    new GameModeOptionResponse
                    {
                        Description = "Number of prompts for each user per round",
                        ResponseType = ResponseType.Integer,
                        DefaultValue = 2,
                        MinValue = 1,
                        MaxValue = 30,
                    },
                    new GameModeOptionResponse
                    {
                        Description = "Number of drawings expected per user",
                        ResponseType = ResponseType.Integer,
                        DefaultValue = 4,
                        MinValue = 2,
                        MaxValue = 10,
                    },
                    new GameModeOptionResponse
                    {
                        Description = "Number of players per prompt",
                        ResponseType = ResponseType.Integer,
                        DefaultValue = 2,
                        MinValue = 2,
                        MaxValue = 10,
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
        public BattleReadyGameMode(Lobby lobby, List<ConfigureLobbyRequest.GameModeOptionRequest> gameModeOptions)
        {
            ValidateOptions(gameModeOptions);

            this.Lobby = lobby;
            int numRounds = (int)gameModeOptions[(int)GameModeOptionsEnum.numRounds].ValueParsed;
            int numPromptsPerUserPerRound = (int)gameModeOptions[(int)GameModeOptionsEnum.numPromptsPerUserPerRound].ValueParsed;
            int expectedDrawingsPerUser = (int)gameModeOptions[(int)GameModeOptionsEnum.numToDraw].ValueParsed;
            int numUsersPerPrompt = (int)gameModeOptions[(int)GameModeOptionsEnum.numPlayersPerPrompt].ValueParsed;
            int gameLength = (int)gameModeOptions[(int)GameModeOptionsEnum.GameLength].ValueParsed;
            TimeSpan? setupDrawingTimer = null;
            TimeSpan? setupPromptTimer = null;
            TimeSpan? creationTimer = null;
            TimeSpan? votingTimer = null;
            
            int numOfEachPartInHand = 3;

            int numPromptsPerRound = (int)Math.Ceiling((double)numPromptsPerUserPerRound * lobby.GetAllUsers().Count / numUsersPerPrompt);

            int minDrawingsRequired = numOfEachPartInHand * 3; // the amount to make one playerHand to give everyone

            int expectedPromptsPerUser = numPromptsPerRound * numRounds / lobby.GetAllUsers().Count;

            if (gameLength > 0)
            {
                setupDrawingTimer = CommonHelpers.GetTimerFromLength(
                    length: (double)gameLength,
                    minTimerLength: BattleReadyConstants.SetupPerDrawingTimerMin * expectedDrawingsPerUser,
                    aveTimerLength: BattleReadyConstants.SetupPerDrawingTimerAve * expectedDrawingsPerUser,
                    maxTimerLength: BattleReadyConstants.SetupPerDrawingTimerMax * expectedDrawingsPerUser);
                setupPromptTimer = CommonHelpers.GetTimerFromLength(
                    length: (double)gameLength,
                    minTimerLength: BattleReadyConstants.SetupPerPromptTimerMin * expectedPromptsPerUser,
                    aveTimerLength: BattleReadyConstants.SetupPerPromptTimerAve * expectedPromptsPerUser,
                    maxTimerLength: BattleReadyConstants.SetupPerPromptTimerMax * expectedPromptsPerUser);
                creationTimer = CommonHelpers.GetTimerFromLength(
                    length: (double)gameLength,
                    minTimerLength: BattleReadyConstants.PerCreationTimerMin * numPromptsPerUserPerRound,
                    aveTimerLength: BattleReadyConstants.PerCreationTimerAve * numPromptsPerUserPerRound,
                    maxTimerLength: BattleReadyConstants.PerCreationTimerMax * numPromptsPerUserPerRound);
                votingTimer = CommonHelpers.GetTimerFromLength(
                    length: (double)gameLength,
                    minTimerLength: BattleReadyConstants.VotingTimerMin,
                    aveTimerLength: BattleReadyConstants.VotingTimerAve,
                    maxTimerLength: BattleReadyConstants.VotingTimerMax);
            }

            StateChain setupDrawing = new StateChain(
                stateGenerator: (int counter) =>
                {
                    if (counter == 0)
                    {
                        return new SetupDrawings_GS(
                            lobby: lobby,
                            drawings: this.Drawings,
                            numExpectedPerUser: expectedDrawingsPerUser,
                            setupDurration: setupDrawingTimer);
                    }
                    if (counter == 1)
                    {
                        int numHeadsNeeded = Math.Max(0, minDrawingsRequired / 3 - this.Drawings.Where(drawing => drawing.Type == DrawingType.Head).ToList().Count);
                        int numBodiesNeeded = Math.Max(0, minDrawingsRequired / 3 - this.Drawings.Where(drawing => drawing.Type == DrawingType.Body).ToList().Count);
                        int numLegsNeeded = Math.Max(0, minDrawingsRequired / 3 - this.Drawings.Where(drawing => drawing.Type == DrawingType.Legs).ToList().Count);

                        if (numHeadsNeeded + numBodiesNeeded + numLegsNeeded > 0)
                        {
                            throw new Exception("Not enough drawings submitted");
                            //todo re add when single user skip is available
                            /*return new ExtraSetupDrawing_GS( 
                                lobby: lobby,
                                drawings: this.Drawings,
                                numHeadsNeeded: numHeadsNeeded,
                                numBodiesNeeded: numBodiesNeeded,
                                numLegsNeeded: numLegsNeeded);*/
                        }
                        else
                        {
                            return null;
                        }
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
                        return new SetupPrompts_GS(
                            lobby: lobby,
                            prompts: Prompts,
                            numExpectedPerUser: expectedPromptsPerUser,
                            setupDurration: setupPromptTimer);
                    }
                    else
                    {
                        return null;
                    }
                });
            List<Prompt> battlePrompts = new List<Prompt>();
            IReadOnlyList<PeopleUserDrawing> headDrawings = new List<PeopleUserDrawing>();
            IReadOnlyList<PeopleUserDrawing> bodyDrawings = new List<PeopleUserDrawing>();
            IReadOnlyList<PeopleUserDrawing> legsDrawings = new List<PeopleUserDrawing>();
            setupDrawing.AddExitListener(() =>
            {
                // Trim extra prompts/drawings.
                headDrawings = MemberHelpers<PeopleUserDrawing>.Select_Ordered(
                    Drawings.ToList().FindAll((drawing) => drawing.Type == DrawingType.Head),
                    expectedDrawingsPerUser * lobby.GetAllUsers().Count / 3);

                bodyDrawings = MemberHelpers<PeopleUserDrawing>.Select_Ordered(
                    Drawings.ToList().FindAll((drawing) => drawing.Type == DrawingType.Body),
                    expectedDrawingsPerUser * lobby.GetAllUsers().Count / 3);

                legsDrawings = MemberHelpers<PeopleUserDrawing>.Select_Ordered(
                    Drawings.ToList().FindAll((drawing) => drawing.Type == DrawingType.Legs),
                    expectedDrawingsPerUser * lobby.GetAllUsers().Count / 3);

            });
            setupPrompt.AddExitListener(() =>
            {
                battlePrompts = MemberHelpers<Prompt>.Select_Ordered(Prompts.ToList(), numPromptsPerRound * numRounds);
                foreach(Prompt prompt in battlePrompts)
                {
                    prompt.MaxMemberCount = numUsersPerPrompt;
                }
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
                List<Prompt> prompts = battlePrompts.Take(numPromptsPerRound).ToList();
                battlePrompts.RemoveRange(0, prompts.Count);

                List<IGroup<User>> assignments = MemberHelpers<User>.Assign(
                    prompts.Cast<IConstraints<User>>().ToList(),
                    lobby.GetAllUsers().ToList(),
                    duplicateMembers: (int)Math.Ceiling((1.0 * prompts.Count / numPromptsPerRound) * numPromptsPerUserPerRound));

                var pairings = prompts.Zip(assignments);

                foreach ((Prompt prompt, IGroup<User> users) in pairings)
                {
                    foreach (User user in users.Members)
                    {
                        prompt.UsersToUserHands.TryAdd(user, new Prompt.UserHand
                        {
                            // Users have even probabilities regardless of how many drawings they submitted.
                            Heads = MemberHelpers<PeopleUserDrawing>.Select_DynamicWeightedRandom(headDrawings, numOfEachPartInHand),
                            Bodies = MemberHelpers<PeopleUserDrawing>.Select_DynamicWeightedRandom(bodyDrawings, numOfEachPartInHand),
                            Legs = MemberHelpers<PeopleUserDrawing>.Select_DynamicWeightedRandom(legsDrawings, numOfEachPartInHand),
                            Contestant = new Person()
                        });

                        if (!RoundTracker.UsersToAssignedPrompts.ContainsKey(user))
                        {
                            RoundTracker.UsersToAssignedPrompts.Add(user, new List<Prompt>());
                        }
                        RoundTracker.UsersToAssignedPrompts[user].Add(prompt);
                    }
                }

                GameState toReturn = new ContestantCreation_GS(
                        lobby: lobby,
                        roundTracker: RoundTracker,
                        creationDuration: creationTimer);
                toReturn.Transition(CreateVotingGameStates(prompts));
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

                    if (battlePrompts.Count <= 0)
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
            //empty
        }
        private State GetVotingAndRevealState(Prompt prompt, TimeSpan? votingTime)
        {
            List<User> randomizedUsersToDisplay = prompt.UsersToUserHands.Keys.OrderBy(_ => Rand.Next()).ToList();
            List<Person> peopleToVoteOn = randomizedUsersToDisplay.Select(user => prompt.UsersToUserHands[user].Contestant).ToList();
            List<string> imageTitles = randomizedUsersToDisplay.Select(user => prompt.UsersToUserHands[user].Contestant.Name).ToList();
            List<string> imageHeaders = randomizedUsersToDisplay.Select(user => user.DisplayName).ToList();

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
                ObjectHeaders = imageHeaders,
                ShowObjectHeadersForVoting = false
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
