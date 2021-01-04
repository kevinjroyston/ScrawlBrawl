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
using Backend.APIs.DataModels.UnityObjects;
using static Backend.Games.BriansGames.BattleReady.DataModels.Prompt;

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
                        Description = "Number of rounds, a round contains numerous battles",
                        ResponseType = ResponseType.Integer,
                        DefaultValue = 2,
                        MinValue = 1,
                        MaxValue = 10,
                    },
                    new GameModeOptionResponse
                    {
                        Description = "Number of contestants each user creates each round",
                        ResponseType = ResponseType.Integer,
                        DefaultValue = 2,
                        MinValue = 1,
                        MaxValue = 30,
                    },
                    new GameModeOptionResponse
                    {
                        Description = "Number of drawings per user during setup",
                        ResponseType = ResponseType.Integer,
                        DefaultValue = 4,
                        MinValue = 2,
                        MaxValue = 10,
                    },
                    new GameModeOptionResponse
                    {
                        Description = "Number of players per battle",
                        ResponseType = ResponseType.Integer,
                        DefaultValue = 3,
                        MinValue = 2,
                        MaxValue = 20,
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
                        int numHeadsNeeded = Math.Max(0, minDrawingsRequired / 3 - this.Drawings.Where(drawing => drawing.Type == BodyPartType.Head).ToList().Count);
                        int numBodiesNeeded = Math.Max(0, minDrawingsRequired / 3 - this.Drawings.Where(drawing => drawing.Type == BodyPartType.Body).ToList().Count);
                        int numLegsNeeded = Math.Max(0, minDrawingsRequired / 3 - this.Drawings.Where(drawing => drawing.Type == BodyPartType.Legs).ToList().Count);

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
                List<PeopleUserDrawing> orderedDrawings = Drawings.OrderBy(drawing => drawing.CreationTime).ToList();
                // Trim extra prompts/drawings.
                headDrawings = MemberHelpers<PeopleUserDrawing>.Select_Ordered(
                    orderedDrawings.FindAll((drawing) => drawing.Type == BodyPartType.Head),
                    expectedDrawingsPerUser * lobby.GetAllUsers().Count / 3);

                bodyDrawings = MemberHelpers<PeopleUserDrawing>.Select_Ordered(
                    orderedDrawings.FindAll((drawing) => drawing.Type == BodyPartType.Body),
                    expectedDrawingsPerUser * lobby.GetAllUsers().Count / 3);

                legsDrawings = MemberHelpers<PeopleUserDrawing>.Select_Ordered(
                    orderedDrawings.FindAll((drawing) => drawing.Type == BodyPartType.Legs),
                    expectedDrawingsPerUser * lobby.GetAllUsers().Count / 3);

            });
            setupPrompt.AddExitListener(() =>
            {
                battlePrompts = MemberHelpers<Prompt>.Select_Ordered(Prompts.OrderBy(prompt=>prompt.CreationTime).ToList(), numPromptsPerRound * numRounds);
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
                            HeadChoices = MemberHelpers<PeopleUserDrawing>.Select_DynamicWeightedRandom(headDrawings, numOfEachPartInHand),
                            BodyChoices = MemberHelpers<PeopleUserDrawing>.Select_DynamicWeightedRandom(bodyDrawings, numOfEachPartInHand),
                            LegChoices = MemberHelpers<PeopleUserDrawing>.Select_DynamicWeightedRandom(legsDrawings, numOfEachPartInHand),
                            Owner = user
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
                    List<Person> winnersPeople = roundPrompts.Select((prompt) => (Person)prompt.UsersToUserHands[prompt.Winner]).ToList();
                    
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
            List<Person> peopleToVoteOn = randomizedUsersToDisplay.Select(user => (Person)prompt.UsersToUserHands[user]).ToList();
            foreach (Person person in peopleToVoteOn)
            {
                person.UnityImageVotingOverrides = new UnityObjectOverrides()
                {
                    Title = person.Name
                };
                person.UnityImageRevealOverrides = new UnityObjectOverrides()
                {
                    Title = person.Name,
                    Header = person.Owner.DisplayName,
                };
            }
            List<string> imageTitles = randomizedUsersToDisplay.Select(user => prompt.UsersToUserHands[user].Name).ToList();
            List<string> imageHeaders = randomizedUsersToDisplay.Select(user => user.DisplayName).ToList();

            var voteAndReveal = new ThreePartPersonVoteAndRevealState<Person>(
                lobby: this.Lobby,
                people: peopleToVoteOn,
                votingTime: votingTime)
            {
                VotingViewOverrides = new UnityViewOverrides
                {
                    Title = prompt.Text,
                },
                RevealViewOverrides = new UnityViewOverrides
                {
                    Title = prompt.Text,
                },
                VoteCountManager = CountVotes
            };
            voteAndReveal.AddExitListener(()=>
            {
                // Determine winner.
                foreach ((User user, UserHand userHand) in prompt.UsersToUserHands)
                {
                    if (prompt.Winner == null || userHand.VotesCastForThisObject.Count > prompt.UsersToUserHands[prompt.Winner].VotesCastForThisObject.Count)
                    {
                        prompt.Winner = user;
                    }
                }
            });
            return voteAndReveal;
        }
        private void CountVotes(List<Person> choices, IDictionary<User, VoteInfo> votes)
        {
            // Points for using drawings.
            foreach (Person person in choices)
            {
                person.BodyPartDrawings[BodyPartType.Head].Owner.ScoreHolder.AddScore(BattleReadyConstants.PointsForPartUsed, Score.Reason.DrawingUsed);
                person.BodyPartDrawings[BodyPartType.Body].Owner.ScoreHolder.AddScore(BattleReadyConstants.PointsForPartUsed, Score.Reason.DrawingUsed);
                person.BodyPartDrawings[BodyPartType.Legs].Owner.ScoreHolder.AddScore(BattleReadyConstants.PointsForPartUsed, Score.Reason.DrawingUsed);
            }

            // Points for vote.
            foreach ((User user, VoteInfo voteInfo) in votes)
            {
                User userVotedFor = ((UserHand)voteInfo.ObjectsVotedFor[0]).Owner;
                userVotedFor.ScoreHolder.AddScore(BattleReadyConstants.PointsForVote, Score.Reason.ReceivedVotes);
            }
        }
    }
}
