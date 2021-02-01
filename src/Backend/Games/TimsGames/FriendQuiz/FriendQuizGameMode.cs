using Backend.GameInfrastructure.DataModels;
using Backend.GameInfrastructure.DataModels.States.GameStates;
using Backend.GameInfrastructure.DataModels.States.StateGroups;
using Backend.GameInfrastructure.DataModels.Users;
using Backend.GameInfrastructure.Extensions;
using Backend.Games.Common.GameStates;
using Backend.Games.TimsGames.FriendQuiz.DataModels;
using Backend.Games.TimsGames.FriendQuiz.GameStates;
using Backend.APIs.DataModels.Exceptions;
using Common.DataModels.Requests.LobbyManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using Common.DataModels.Responses;
using Backend.GameInfrastructure;
using Common.DataModels.Enums;
using Backend.Games.Common;
using Common.DataModels.Interfaces;
using Common.Code.Helpers;
using Backend.Games.Common.GameStates.QueryAndReveal;
using Backend.APIs.DataModels.UnityObjects;

namespace Backend.Games.TimsGames.FriendQuiz
{
    public class FriendQuizGameMode: IGameMode
    {
        private Random Rand { get; } = new Random();
        private GameState Setup { get; set; }
        private RoundTracker RoundTracker { get; set; } = new RoundTracker();
        public static GameModeMetadata GameModeMetadata { get; } = new GameModeMetadata
        {
            Title = "Friend Quiz",
            GameId = GameModeId.FriendQuiz,
            Description = "See how well you know your fellow players",
            MinPlayers = 3,
            MaxPlayers = null,
            Attributes = new GameModeAttributes
            {
                ProductionReady = false,
            },
            Options = new List<GameModeOptionResponse>
                {
                    new GameModeOptionResponse
                    {
                        Description = "Number of questions created by each user",
                        ResponseType = ResponseType.Integer,
                        DefaultValue = 2,
                        MinValue = 1,
                        MaxValue = 5,
                    },
                    new GameModeOptionResponse
                    {
                        Description = "Number of questions answered by each user",
                        ResponseType = ResponseType.Integer,
                        DefaultValue = 5,
                        MinValue = 1,
                        MaxValue = 10,
                    },

                    /*new GameModeOptionResponse
                    {
                        Description = "Min number of questions to show for voting",
                        ResponseType = ResponseType.Integer,
                        DefaultValue = 3,
                        MinValue = 1,
                        MaxValue = 30,
                    },
                    new GameModeOptionResponse
                    {
                        Description = "Outlier Extra Round",
                        ResponseType = ResponseType.Boolean,
                        DefaultValue = true,
                    },*/
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
        public FriendQuizGameMode(Lobby lobby, List<ConfigureLobbyRequest.GameModeOptionRequest> gameModeOptions)
        {
            int numQuestionSetup = (int)gameModeOptions[(int)GameModeOptionsEnum.NumSubmitQuestions].ValueParsed;
            int numQuestionsToAnswer = (int)gameModeOptions[(int)GameModeOptionsEnum.NumAnswerQuestions].ValueParsed;
            //int minQuestions = (int)gameModeOptions[(int)GameModeOptionsEnum.MinQuestions].ValueParsed;
            //bool outlierExtraRound = (bool)gameModeOptions[(int)GameModeOptionsEnum.OutlierExtraRound].ValueParsed;
            int gameLength = (int)gameModeOptions[(int)GameModeOptionsEnum.GameLength].ValueParsed;

            TimeSpan? setupTimer = null;
            TimeSpan? answeringTimer = null;
            TimeSpan? votingTimer = null;

            if (gameLength > 0)
            {
                setupTimer = CommonHelpers.GetTimerFromLength(
                    length: (double)gameLength,
                    minTimerLength: FriendQuizConstants.SetupTimerMin,
                    aveTimerLength: FriendQuizConstants.SetupTimerAve,
                    maxTimerLength: FriendQuizConstants.SetupTimerMax);
                answeringTimer = CommonHelpers.GetTimerFromLength(
                    length: (double)gameLength,
                    minTimerLength: FriendQuizConstants.AnsweringTimerMin,
                    aveTimerLength: FriendQuizConstants.AnsweringTimerAve,
                    maxTimerLength: FriendQuizConstants.AnsweringTimerMax);
                votingTimer = CommonHelpers.GetTimerFromLength(
                    length: (double)gameLength,
                    minTimerLength: FriendQuizConstants.VotingTimerMin,
                    aveTimerLength: FriendQuizConstants.VotingTimerAve,
                    maxTimerLength: FriendQuizConstants.VotingTimerMax);
                
            }

            //numQuestionsToAnswer = Math.Clamp(numQuestionsToAnswer, 1, lobby.GetAllUsers().Count * numQuestionSetup);
            Dictionary<User, List<Question>> usersToAssignedQuestions = new Dictionary<User, List<Question>>();
            int maxNumAssigned = 1;

            StateChain gameStateChain = new StateChain(stateGenerator: (int counter) =>
            {
                if (counter == 0)
                {
                    setupTimer = setupTimer?.Multiply(numQuestionSetup);
                    return new Setup_GS(
                        lobby: lobby,
                        roundTracker: RoundTracker,
                        numExpectedPerUser: numQuestionSetup,
                        setupDuration: setupTimer);
                }
                else if (counter == 1)
                {
                    List<UserQuestionsHolder> userQuestionsHolders = lobby.GetAllUsers().Select(user => new UserQuestionsHolder(user, numQuestionsToAnswer)).ToList();
                    List<Question> randomizedQuestions = RoundTracker.Questions.OrderBy(_ => Rand.Next()).ToList();
                    List<IGroup<Question>> assignments = MemberHelpers<Question>.Assign(userQuestionsHolders.Cast<IConstraints<Question>>().ToList(), randomizedQuestions, lobby.GetAllUsers().Count);

                    var pairings = userQuestionsHolders.Zip(assignments);

                    foreach ((UserQuestionsHolder holder, IGroup<Question> questions) in pairings)
                    {
                        if (questions.Members.Count() > maxNumAssigned)
                        {
                            maxNumAssigned = questions.Members.Count();
                        }
                        // Makes a copy of the questions so that it can handle multiple people answering the same question without them both overriding the same object
                        usersToAssignedQuestions.Add(holder.QuestionedUser, questions.Members.Select(question => new Question(question) { MainUser = holder.QuestionedUser }).ToList());
                    }

                    answeringTimer = answeringTimer?.Multiply(maxNumAssigned);

                    return new AnswerQuestion_GS(
                        lobby: lobby,
                        usersToAssignedQuestions: usersToAssignedQuestions,
                        answerTimeDuration: answeringTimer);
                }
                else if (counter == 2)
                {
                    votingTimer = votingTimer?.Multiply(maxNumAssigned);

                    List<User> randomizedUsers = usersToAssignedQuestions.Keys.OrderBy(_ => Rand.Next()).ToList();

                    return GetUserQueryStateChain(randomizedUsers);
                }
                else
                {
                    return null;
                }
            });

            
            this.Entrance.Transition(gameStateChain);
            gameStateChain.Transition(this.Exit);

            StateChain GetUserQueryStateChain(List<User> users)
            {
                List<State> chain = new List<State>();
                foreach (User user in users)
                {
                    List<Question> nonAbstainedQuestions = usersToAssignedQuestions[user].Where(question => !question.Abstained).ToList();
                    if (nonAbstainedQuestions.Count > 0)
                    {
                        chain.Add(new StateChain(stateGenerator: (int counter) =>
                        {
                            if (counter == 0)
                            {
                                return new SliderQueryAndReveal(
                                   lobby: lobby,
                                   objectsToQuery: nonAbstainedQuestions,
                                   usersToQuery: lobby.GetAllUsers().Where(lobbyUser => lobbyUser != user).ToList(),
                                   queryTime: votingTimer)
                                {
                                    QueryPromptTitle = $"How do you think {user.DisplayName} answered these questions?",
                                    QueryPromptDescription = "The tighter the range of your guess, the more points if you're correct",
                                    QueryViewOverrides = new UnityViewOverrides()
                                    {
                                        Title = $"How do you think {user.DisplayName} answered these questions?",
                                    },
                                    RevealViewOverrides = new UnityViewOverrides()
                                    {
                                        Title = $"This is how {user.DisplayName} answered those questions.",
                                    },
                                    QueryExitListener = CountQueries,
                                };
                            }
                            else if (counter == 1)
                            {
                                if (user == users.Last())
                                {
                                    return new ScoreBoardGameState(lobby, "Final Scores");
                                }
                                else
                                {
                                    return new ScoreBoardGameState(lobby);
                                }
                            }
                            else
                            {
                                return null;
                            }
                        }));
                    }
                }
                return new StateChain(chain);
            }
        }
        private void CountQueries(List<Question> questions)
        {
            foreach(Question question in questions)
            {
                foreach(QueryInfo<(int, int)> queryInfo in question.UserAnswers)
                {
                    (int, int) answer = queryInfo.Answer;
                    if (answer.Item1 <= question.MainAnswer && question.MainAnswer <= answer.Item2)
                    {
                        queryInfo.UserQueried.ScoreHolder.AddScore(
                            amount: CalculateScore(
                                question: question,
                                ansMin: answer.Item1,
                                ansMax: answer.Item2),
                            reason: Score.Reason.CorrectAnswer);
                    }
                }
            }
        }

        private int CalculateScore(Question question, int ansMin, int ansMax)
        {
            double rangeInverse = Math.Pow(1.0 - 1.0 * (ansMax - ansMin) / (question.MaxBound - question.MinBound), 3);
            return (int) (rangeInverse * FriendQuizConstants.PointsForCorrectAnswer);
        }
    }
}
