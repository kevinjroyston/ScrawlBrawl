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
using Common.Code.Extensions;
using MiscUtil;

namespace Backend.Games.BriansGames.BattleReady
{
    public class BattleReadyGameMode : IGameMode
    {
        private ConcurrentBag<PeopleUserDrawing> Drawings { get; set; } = new ConcurrentBag<PeopleUserDrawing>();
        private Lobby Lobby { get; set; }
        private ConcurrentBag<Prompt> Prompts { get; set; } = new ConcurrentBag<Prompt>();
        private RoundTracker RoundTracker { get; } = new RoundTracker();
        private Random Rand { get; } = new Random();

        private const int MinPlayers = 3;

        public static GameModeMetadata GameModeMetadata { get; } = new GameModeMetadata
        {
            Title = "Body Builder", // in code refered to as Battle Ready
            GameId = GameModeId.BodyBuilder,
            Description = "Go head to head body to body and legs to legs with other players to try to make the best constestant for each challenge.",
            MinPlayers = MinPlayers,
            MaxPlayers = CommonGameConstants.MAX_PLAYERS,
            Attributes = new GameModeAttributes
            {
                ProductionReady = true,
            },
            Options = new List<GameModeOptionResponse>
            {
            },
            GetGameDurationEstimates = GetGameDurationEstimates,
        };
        private static IReadOnlyDictionary<GameDuration, TimeSpan> GetGameDurationEstimates(int numPlayers, List<ConfigureLobbyRequest.GameModeOptionRequest> gameModeOptions)
        {
            Dictionary<GameDuration, TimeSpan> estimates = new Dictionary<GameDuration, TimeSpan>();
            numPlayers = Math.Max(numPlayers, MinPlayers);
            foreach (GameDuration duration in Enum.GetValues(typeof(GameDuration)))
            {
                int numRounds = BattleReadyConstants.NumRounds[duration];
                int numPromptsPerRound = Math.Min(numPlayers, BattleReadyConstants.MaxNumSubRounds[duration]);
                int minDrawingsRequired = BattleReadyConstants.NumDrawingsInHand * 3; // the amount to make one playerHand to give everyone

                int expectedPromptsPerUser = (int)Math.Ceiling(1.0 * numPromptsPerRound * numRounds / numPlayers);
                int expectedDrawingsPerUser = Math.Max((minDrawingsRequired / numPlayers) * 2, BattleReadyConstants.NumDrawingsPerPlayer[duration]);
                
                int numPromptsPerUserPerRound = Math.Max(1, numPromptsPerRound / 2);

                TimeSpan estimate = TimeSpan.Zero;
                TimeSpan setupDrawingTimer = BattleReadyConstants.SetupPerDrawingTimer[duration];
                TimeSpan setupPromptTimer = BattleReadyConstants.SetupPerPromptTimer[duration];
                TimeSpan creationTimer = BattleReadyConstants.PerCreationTimer[duration];
                TimeSpan votingTimer = BattleReadyConstants.VotingTimer[duration];

                estimate += setupDrawingTimer.MultipliedBy(expectedDrawingsPerUser);
                estimate += setupPromptTimer.MultipliedBy(expectedPromptsPerUser);
                estimate += creationTimer.MultipliedBy(numPromptsPerUserPerRound * numRounds);
                estimate += votingTimer.MultipliedBy(numPromptsPerRound * numRounds);
                estimates[duration] = estimate;
            }

            return estimates;
        }
        public BattleReadyGameMode(Lobby lobby, List<ConfigureLobbyRequest.GameModeOptionRequest> gameModeOptions, StandardGameModeOptions standardOptions)
        {
            GameDuration duration = standardOptions.GameDuration;

            this.Lobby = lobby;
            int numRounds = BattleReadyConstants.NumRounds[duration];
            int numPlayers = lobby.GetAllUsers().Count();

            TimeSpan? setupDrawingTimer = null;
            TimeSpan? setupPromptTimer = null;
            TimeSpan? creationTimer = null;
            TimeSpan? votingTimer = null;

            int numPromptsPerRound = Math.Min(numPlayers, BattleReadyConstants.MaxNumSubRounds[duration]);
            int minDrawingsRequired = BattleReadyConstants.NumDrawingsInHand * 3; // the amount to make one playerHand to give everyone

            int expectedPromptsPerUser = (int) Math.Ceiling(1.0*numPromptsPerRound * numRounds / lobby.GetAllUsers().Count);
            int expectedDrawingsPerUser = Math.Max((minDrawingsRequired / numPlayers) * 2, BattleReadyConstants.NumDrawingsPerPlayer[duration]);

            if (standardOptions.TimerEnabled)
            {
                setupDrawingTimer = BattleReadyConstants.SetupPerDrawingTimer[duration];
                setupPromptTimer = BattleReadyConstants.SetupPerPromptTimer[duration];
                creationTimer = BattleReadyConstants.PerCreationTimer[duration];
                votingTimer = BattleReadyConstants.VotingTimer[duration];
            }

            SetupDrawings_GS setupDrawing = new SetupDrawings_GS(
                lobby: lobby,
                drawings: this.Drawings,
                numExpectedPerUser: expectedDrawingsPerUser,
                setupDurration: setupDrawingTimer);

            SetupPrompts_GS setupPrompt = new SetupPrompts_GS(
                lobby: lobby,
                prompts: Prompts,
                numExpectedPerUser: expectedPromptsPerUser,
                setupDuration: setupPromptTimer);

            List<Prompt> battlePrompts = new List<Prompt>();
            IReadOnlyList<PeopleUserDrawing> headDrawings = new List<PeopleUserDrawing>();
            IReadOnlyList<PeopleUserDrawing> bodyDrawings = new List<PeopleUserDrawing>();
            IReadOnlyList<PeopleUserDrawing> legsDrawings = new List<PeopleUserDrawing>();
            setupDrawing.AddExitListener(() =>
            {
                // Trim extra prompts/drawings.
                headDrawings = Drawings.ToList().FindAll((drawing) => drawing.Type == BodyPartType.Head);
                bodyDrawings = Drawings.ToList().FindAll((drawing) => drawing.Type == BodyPartType.Body);
                legsDrawings = Drawings.ToList().FindAll((drawing) => drawing.Type == BodyPartType.Legs);
            });
            int numPromptsPerUserPerRound = 0; // Set during below exit listener.
            setupPrompt.AddExitListener(() =>
            {
                battlePrompts = MemberHelpers<Prompt>.Select_Ordered(Prompts.OrderBy(prompt=>prompt.CreationTime).ToList(), numPromptsPerRound * numRounds);
                numRounds = (battlePrompts.Count - 1) / numPromptsPerRound + 1;
                numPromptsPerRound = (int)Math.Ceiling(1.0 * battlePrompts.Count / numRounds);

                numPromptsPerUserPerRound = Math.Max(1,numPromptsPerRound / 2);
                int maxNumUsersPerPrompt = Math.Min(12,(int)Math.Ceiling(1.0*numPlayers * numPromptsPerUserPerRound / numPromptsPerRound));

                foreach (Prompt prompt in battlePrompts)
                {
                    prompt.MaxMemberCount = maxNumUsersPerPrompt;
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
                            // Users who submit more drawings technically have higher chances of being seen. (Used to use Select_DynamicWeightedRandom which kept author counts fair, but could skew some drawings to being seen more as a result)
                            HeadChoices = headDrawings.OrderBy((val) => StaticRandom.Next()).ToList().Take(BattleReadyConstants.NumDrawingsInHand).ToList(),
                            BodyChoices = bodyDrawings.OrderBy((val) => StaticRandom.Next()).ToList().Take(BattleReadyConstants.NumDrawingsInHand).ToList(),
                            LegChoices = legsDrawings.OrderBy((val) => StaticRandom.Next()).ToList().Take(BattleReadyConstants.NumDrawingsInHand).ToList(),
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
                        title: "This round's winning contestants",
                        peopleList: winnersPeople,
                        imageTitle: (person) => roundPrompts[winnersPeople.IndexOf(person)].Text,
                        imageHeader: (person) => person.Name
                        );

                    if (battlePrompts.Count <= 0)
                    {
                        GameState finalScoreBoard = new ScoreBoardGameState(
                        lobby: lobby,
                        title: "Final Top Scores");
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

            var voteAndReveal = new ContestantVoteAndRevealState<Person>(
                lobby: this.Lobby,
                contestantName: (person) => person.Name,
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
                if (person.BodyPartDrawings[BodyPartType.Head].Owner != person.Owner) person.BodyPartDrawings[BodyPartType.Head].Owner.ScoreHolder.AddScore(BattleReadyConstants.PointsForPartUsed, Score.Reason.DrawingUsed);
                if (person.BodyPartDrawings[BodyPartType.Body].Owner != person.Owner) person.BodyPartDrawings[BodyPartType.Body].Owner.ScoreHolder.AddScore(BattleReadyConstants.PointsForPartUsed, Score.Reason.DrawingUsed);
                if (person.BodyPartDrawings[BodyPartType.Legs].Owner != person.Owner) person.BodyPartDrawings[BodyPartType.Legs].Owner.ScoreHolder.AddScore(BattleReadyConstants.PointsForPartUsed, Score.Reason.DrawingUsed);
            }

            // Points for vote.
            foreach ((User user, VoteInfo voteInfo) in votes)
            {
                User userVotedFor = ((UserHand)voteInfo.ObjectsVotedFor[0]).Owner;
                if (userVotedFor != user) userVotedFor.ScoreHolder.AddScore(BattleReadyConstants.PointsForVote, Score.Reason.ReceivedVotes);
            }
        }
    }
}
