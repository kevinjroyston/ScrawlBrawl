using Backend.GameInfrastructure.DataModels.States.GameStates;
using Backend.GameInfrastructure.Extensions;
using Backend.Games.KevinsGames.TextBodyBuilder.DataModels;
using Backend.Games.KevinsGames.TextBodyBuilder.GameStates;
using Common.DataModels.Requests.LobbyManagement;
using System.Collections.Generic;
using Backend.Games.Common.GameStates;
using Backend.GameInfrastructure.DataModels.Users;
using System;
using System.Linq;
using Backend.GameInfrastructure.DataModels.States.StateGroups;
using System.Collections.Concurrent;
using Backend.Games.Common.GameStates.VoteAndReveal;
using Backend.GameInfrastructure.DataModels;
using Common.DataModels.Responses;
using Microsoft.CodeAnalysis;
using Backend.GameInfrastructure;
using Common.DataModels.Enums;
using Common.Code.Helpers;
using Common.DataModels.Interfaces;
using Backend.APIs.DataModels.UnityObjects;
using static Backend.Games.KevinsGames.TextBodyBuilder.DataModels.Prompt;
using Common.Code.Extensions;
using static Backend.Games.KevinsGames.TextBodyBuilder.DataModels.TextPerson;
using static System.FormattableString;
using Backend.APIs.DataModels.Enums;
using Backend.Games.Common;

namespace Backend.Games.KevinsGames.TextBodyBuilder.Game
{
    public class TextBodyBuilderGameMode : IGameMode
    {
        private ConcurrentBag<CAMUserText> CAMs { get; set; } = new ConcurrentBag<CAMUserText>();
        private Lobby Lobby { get; set; }
        private ConcurrentBag<Prompt> Prompts { get; set; } = new ConcurrentBag<Prompt>();
        private RoundTracker RoundTracker { get; } = new RoundTracker();
        private Random Rand { get; } = new Random();

        private const int MinPlayers = 3;
        public static GameModeMetadata GameModeMetadata { get; } = new GameModeMetadata
        {
            Title = "Character Builder", 
            GameId = GameModeId.TextBodyBuilder,
            Description = "Combine snippets of text to bring to life the best constestant for each challenge.",
            MinPlayers = MinPlayers,
            MaxPlayers = CommonGameConstants.MAX_PLAYERS,
            Attributes = new GameModeAttributes
            {
                ProductionReady = false,
            },
            Options = new List<GameModeOptionResponse>
            {
            },
            GetGameDurationEstimates = GetGameDurationEstimates,

        };
        private static IReadOnlyDictionary<GameDuration, TimeSpan> GetGameDurationEstimates(int numPlayers, List<ConfigureLobbyRequest.GameModeOptionRequest> gameModeOptions)
        {
            numPlayers = Math.Max(numPlayers, MinPlayers);
            Dictionary<GameDuration, TimeSpan> estimates = new Dictionary<GameDuration, TimeSpan>();
            foreach (GameDuration duration in Enum.GetValues(typeof(GameDuration)))
            {
                int numRounds = TextBodyBuilderConstants.NumRounds[duration];
                int numPromptsPerRound = Math.Min(numPlayers, TextBodyBuilderConstants.MaxNumSubRounds[duration]);
                int minCAMsRequired = TextBodyBuilderConstants.NumCAMsInHand * SetupCAMs_GS.UserGeneratedCamTypes.Count; // the amount to make one playerHand to give everyone

                int expectedPromptsPerUser = (int)Math.Ceiling(1.0 * numPromptsPerRound * numRounds / numPlayers);
                int expectedCAMsPerUser = Math.Max((minCAMsRequired / numPlayers + 1) * 2, TextBodyBuilderConstants.NumCAMsPerPlayer[duration]);

                int numPromptsPerUserPerRound = Math.Max(1, (int)Math.Ceiling(numPromptsPerRound * 1.5 / numPlayers));

                TimeSpan estimate = TimeSpan.Zero;
                TimeSpan setupDrawingTimer = TextBodyBuilderConstants.SetupPerCAMTimer[duration];
                TimeSpan setupPromptTimer = TextBodyBuilderConstants.SetupPerPromptTimer[duration];
                TimeSpan creationTimer = TextBodyBuilderConstants.PerCreationTimer[duration];
                TimeSpan votingTimer = TextBodyBuilderConstants.VotingTimer[duration];

                estimate += setupDrawingTimer.MultipliedBy(expectedCAMsPerUser);
                estimate += setupPromptTimer.MultipliedBy(expectedPromptsPerUser);
                estimate += creationTimer.MultipliedBy(numPromptsPerUserPerRound * numRounds);
                estimate += votingTimer.MultipliedBy(numPromptsPerRound * numRounds);
                estimates[duration] = estimate;
            }

            return estimates;
        }
        public TextBodyBuilderGameMode(Lobby lobby, List<ConfigureLobbyRequest.GameModeOptionRequest> gameModeOptions, StandardGameModeOptions standardOptions)
        {
            GameDuration duration = standardOptions.GameDuration;

            this.Lobby = lobby;
            int numRounds = TextBodyBuilderConstants.NumRounds[duration];
            int numPlayers = lobby.GetAllUsers().Count();

            TimeSpan? setupDrawingTimer = null;
            TimeSpan? setupPromptTimer = null;
            TimeSpan? creationTimer = null;
            TimeSpan? votingTimer = null;

            int numPromptsPerRound = Math.Min(numPlayers, TextBodyBuilderConstants.MaxNumSubRounds[duration]);
            int minCAMsRequired = TextBodyBuilderConstants.NumCAMsInHand * SetupCAMs_GS.UserGeneratedCamTypes.Count; // the amount to make one playerHand to give everyone

            int expectedPromptsPerUser = (int) Math.Ceiling(1.0*numPromptsPerRound * numRounds / numPlayers);
            int expectedCAMsPerUser = Math.Max((minCAMsRequired / numPlayers + 1) * 2, TextBodyBuilderConstants.NumCAMsPerPlayer[duration]);

            if (standardOptions.TimerEnabled)
            {
                setupDrawingTimer = TextBodyBuilderConstants.SetupPerCAMTimer[duration];
                setupPromptTimer = TextBodyBuilderConstants.SetupPerPromptTimer[duration];
                creationTimer = TextBodyBuilderConstants.PerCreationTimer[duration];
                votingTimer = TextBodyBuilderConstants.VotingTimer[duration];
            }

            SetupCAMs_GS setupDrawing = new SetupCAMs_GS(
                lobby: lobby,
                cams: this.CAMs,
                numExpectedPerUser: expectedCAMsPerUser,
                setupDurration: setupDrawingTimer * expectedCAMsPerUser);

            SetupPrompts_GS setupPrompt = new SetupPrompts_GS(
                lobby: lobby,
                prompts: Prompts,
                numExpectedPerUser: expectedPromptsPerUser,
                setupDuration: setupPromptTimer);

            List<Prompt> battlePrompts = new List<Prompt>();
            IReadOnlyList<CAMUserText> characters = new List<CAMUserText>();
            IReadOnlyList<CAMUserText> actions = new List<CAMUserText>();
            IReadOnlyList<CAMUserText> modifiers = new List<CAMUserText>();
            setupDrawing.AddExitListener(() =>
            {
                // Trim extra prompts/drawings.
                characters = CAMs.ToList().FindAll((cam) => cam.Type == CAMType.Character);
                actions = CAMs.ToList().FindAll((cam) => cam.Type == CAMType.Action);
                modifiers = CAMs.ToList().FindAll((cam) => cam.Type == CAMType.Modifier);
            });
            int numPromptsPerUserPerRound = 0; // Set during below exit listener.
            int actualNumRounds = 0;
            setupPrompt.AddExitListener(() =>
            {
                battlePrompts = MemberHelpers<Prompt>.Select_Ordered(Prompts.OrderBy(prompt=>prompt.CreationTime).ToList(), numPromptsPerRound * numRounds);
                actualNumRounds = battlePrompts.Count;
                numRounds = (battlePrompts.Count - 1) / numPromptsPerRound + 1;
                numPromptsPerRound = (int)Math.Ceiling(1.0 * battlePrompts.Count / numRounds);

                numPromptsPerUserPerRound = Math.Max(1, (int)Math.Ceiling(numPromptsPerRound * 1.5 / numPlayers));
                int maxNumUsersPerPrompt = Math.Max(2,Math.Min(4,(int)Math.Ceiling(1.0*numPlayers * numPromptsPerUserPerRound / numPromptsPerRound)));

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
                    if (users == null)
                    {
                        continue;
                    }
                    foreach (User user in users.Members)
                    {
                        prompt.UsersToUserHands.TryAdd(user, new Prompt.UserHand
                        {
                            // Users who submit more CAMs technically have higher chances of being seen. (Used to use Select_DynamicWeightedRandom which kept author counts fair, but could skew some CAMs to being seen more as a result)
                            CharacterChoices = characters.OrderBy((val) => StaticRandom.Next()).Take(TextBodyBuilderConstants.NumCAMsInHand).ToList(),
                            ActionChoices = actions.OrderBy((val) => StaticRandom.Next()).Take(TextBodyBuilderConstants.NumCAMsInHand).ToList(),
                            //ModifierChoices = MemberHelpers<CAMUserText>.Select_DynamicWeightedRandom(modifiers, TextBodyBuilderConstants.NumCAMsInHand),
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
            int roundCounter = 1;
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

                                return GetVotingAndRevealState(
                                    roundPrompt,
                                    votingTimer,
                                    new UnityRoundDetails
                                    { 
                                        CurrentRound = roundCounter++,
                                        TotalRounds = actualNumRounds
                                    });         
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
                    List<TextPerson> winnersPeople = roundPrompts.Select((prompt) => (TextPerson)prompt.UsersToUserHands[prompt.Winner]).ToList();
                    
                    countRounds++;
                    GameState displayPeople = new DisplayContestants_GS<TextPerson>(
                        lobby: lobby,
                        title: "Here are your winners",
                        peopleList: winnersPeople,
                        imageTitle: (person) => roundPrompts[winnersPeople.IndexOf(person)].Text,
                        imageHeader: (person) => person.ToUnityRichTextString()
                        );

                    if (battlePrompts.Count <= 0)
                    {
                        GameState finalScoreBoard = new ScoreBoardGameState(
                            lobby: lobby);
                        displayPeople.Transition(finalScoreBoard);
                        finalScoreBoard.Transition(this.Exit);
                    }
                    else
                    {
                        GameState scoreBoard = new ScoreBoardGameState(
                            lobby: lobby,
                            revealing: false);
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
        private State GetVotingAndRevealState(Prompt prompt, TimeSpan? votingTime, UnityRoundDetails roundDetails)
        {
            List<User> randomizedUsersToDisplay = prompt.UsersToUserHands.Keys.OrderBy(_ => Rand.Next()).ToList();
            List<TextPerson> peopleToVoteOn = randomizedUsersToDisplay.Select(user => (TextPerson)prompt.UsersToUserHands[user]).ToList();
            foreach (TextPerson person in peopleToVoteOn)
            {
                person.UnityImageVotingOverrides = new UnityObjectOverrides()
                {
                    Title = person.ToUnityRichTextString(),
                };
                person.UnityImageRevealOverrides = new UnityObjectOverrides()
                {
                    Title = person.ToUnityRichTextString(),
                    Header = person.Owner.DisplayName,
                };
            }

            var voteAndReveal = new ContestantVoteAndRevealState<TextPerson>(
                lobby: this.Lobby,
                contestantName: (person) => person.ToHtmlColoredString(),
                people: peopleToVoteOn,
                votingTime: votingTime,
                roundDetails: roundDetails)
            {
                VotingPromptTitle = Invariant($"Pick the best submission for<div class='votePrompt'>{prompt.Text}</div>"),
                VotingViewOverrides = new UnityViewOverrides
                {
                    Title = prompt.Text,
                    Options = new Dictionary<UnityViewOptions, object>
                    {
                        { UnityViewOptions.PrimaryAxis, Axis.Vertical },
                        //{ UnityViewOptions.PrimaryAxisMaxCount, 7 },
                    }
                },
                RevealViewOverrides = new UnityViewOverrides
                {
                    Title = prompt.Text,
                    Options = new Dictionary<UnityViewOptions, object>
                    {
                        { UnityViewOptions.PrimaryAxis, Axis.Vertical },
                        //{ UnityViewOptions.PrimaryAxisMaxCount, 7 },
                    }
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
        private void CountVotes(List<TextPerson> choices, IDictionary<User, VoteInfo> votes)
        {
            // Points for using drawings.
            foreach (TextPerson person in choices)
            {
                if (person.Descriptors[CAMType.Character].Owner != person.Owner) person.Descriptors[CAMType.Character].Owner.ScoreHolder.AddScore(TextBodyBuilderConstants.PointsForPartUsed, Score.Reason.DescriptorUsed);
                if (person.Descriptors[CAMType.Action].Owner != person.Owner) person.Descriptors[CAMType.Action].Owner.ScoreHolder.AddScore(TextBodyBuilderConstants.PointsForPartUsed, Score.Reason.DescriptorUsed);
                if (person.Descriptors[CAMType.Modifier].Owner != person.Owner) person.Descriptors[CAMType.Modifier].Owner.ScoreHolder.AddScore(TextBodyBuilderConstants.PointsForPartUsed, Score.Reason.DescriptorUsed);
            }

            // Points for vote.
            foreach ((User user, VoteInfo voteInfo) in votes)
            {
                User userVotedFor = ((UserHand)voteInfo.ObjectsVotedFor[0]).Owner;
                if (userVotedFor != user) userVotedFor.ScoreHolder.AddScore(TextBodyBuilderConstants.PointsForVote, Score.Reason.ReceivedVotes);
            }

            // This is probably just equal to num players -1.
            int totalOtherVotes = (choices.Sum((person) => person.VotesCastForThisObject.Count)) - 1;
            // Points for voting with crowd.
            foreach (TextPerson person in choices)
            {
                // Gives a percentage of voting with crowd points. Linear with the percentage of other players who agreed with you.
                int scorePerPlayer = (int)(TextBodyBuilderConstants.MaxPointsForVotingWithCrowd * ((person.VotesCastForThisObject.Count - 1) / 1.0 / totalOtherVotes));
                foreach (User userWhoVoted in person.VotesCastForThisObject.Select(vote => vote.UserWhoVoted))
                {
                    userWhoVoted.ScoreHolder.AddScore(scorePerPlayer, Score.Reason.VotedWithCrowd);
                }
            }
        }
    }
}
