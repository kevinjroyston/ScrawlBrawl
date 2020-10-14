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
            GameId = GameModeId.FriendQuiz.ToString(),
            Description = "See how well you know your fellow players",
            MinPlayers = 3,
            MaxPlayers = null,
            Options = new List<GameModeOptionResponse>
                {
                    new GameModeOptionResponse
                    {
                        Description = "Max number of questions to show for voting",
                        ResponseType = ResponseType.Integer,
                        DefaultValue = 5,
                        MinValue = 1,
                        MaxValue = 30,
                    },
                    new GameModeOptionResponse
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
        public FriendQuizGameMode(Lobby lobby, List<ConfigureLobbyRequest.GameModeOptionRequest> gameModeOptions)
        {
            ValidateOptions(gameModeOptions);
            int maxQuestions = (int)gameModeOptions[(int)GameModeOptionsEnum.MaxQuestions].ValueParsed;
            int minQuestions = (int)gameModeOptions[(int)GameModeOptionsEnum.MinQuestions].ValueParsed;
            bool outlierExtraRound = (bool)gameModeOptions[(int)GameModeOptionsEnum.OutlierExtraRound].ValueParsed;
            float setupTimerLength = (int)gameModeOptions[(int)GameModeOptionsEnum.SetupTimerLength].ValueParsed;
            float answerTimerLength = (int)gameModeOptions[(int)GameModeOptionsEnum.AnswerTimerLength].ValueParsed;
            float votingTimerLength = (int)gameModeOptions[(int)GameModeOptionsEnum.VotingTimerLength].ValueParsed;

            if(maxQuestions > lobby.GetAllUsers().Count)
            {
                maxQuestions = lobby.GetAllUsers().Count;
            }
            if (minQuestions > lobby.GetAllUsers().Count)
            {
                minQuestions = lobby.GetAllUsers().Count;
            }

            Setup = new Setup_GS(
                lobby: lobby,
                roundTracker: RoundTracker,
                writingDuration: TimeSpan.FromSeconds(setupTimerLength));

            StateChain CreateGamePlayLoop()
            {
                List<Question> randomizedQuestions = RoundTracker.Questions.OrderBy(_ => Rand.Next()).ToList();
                List<User> randomizedUsers = lobby.GetAllUsers().OrderBy(_ => Rand.Next()).ToList();
                bool timeForScore = true;
                StateChain gamePlayLoop = new StateChain(stateGenerator: (int counter) =>
                {
                    if (counter == 0)
                    {
                        return GetAnsweringStateChain(randomizedQuestions: randomizedQuestions);
                    }
                    else if (counter < randomizedUsers.Count + 1)
                    {
                        User userToShow = randomizedUsers[counter - 1];
                        List<Question> questionsToShow = new List<Question>();
                        foreach (Question question in randomizedQuestions)
                        {
                            if (question.UsersToAnswers.ContainsKey(userToShow) && question.UsersToAnswers[userToShow] != 0 && questionsToShow.Count < maxQuestions)
                            {
                                questionsToShow.Add(question);
                            }
                        }

                        if (questionsToShow.Count < minQuestions) // not enough complete answers to continue
                        {
                            return new StateChain(stateGenerator: (int counter) =>
                            {
                                return null;
                            }); //gives an empty state chain so it skips
                        }
                        else
                        {
                            return GetVotingStateChain(questionsToShow, userToShow);
                        }
                        
                    }
                    else
                    {
                        if (timeForScore)
                        {
                            timeForScore = false;
                            return new ScoreBoardGameState(lobby, "Final Scores");
                        }
                        else
                        {
                            return null;
                        }
                    }
                });
                gamePlayLoop.Transition(this.Exit);
                return gamePlayLoop;
            }
            this.Entrance.Transition(Setup);
            Setup.Transition(CreateGamePlayLoop);

            StateChain GetVotingStateChain(List<Question> questionsToShow, User userToShow)
            {
                return new StateChain(
                    states: new List<State>() {
                        new Voting_GS(
                            lobby: lobby,
                            questionsToShow: questionsToShow,
                            userToShow: userToShow,
                            votingTime: TimeSpan.FromSeconds(votingTimerLength)),
                        new VoteRevealed_GS(
                            lobby: lobby,
                            questionsToShow: questionsToShow,
                            userToShow: userToShow)});
            }

            StateChain GetAnsweringStateChain(List<Question> randomizedQuestions)
            {
                return new StateChain(
                    stateGenerator: (int counter) =>
                    {
                        if(counter == 0)
                        {
                            return new Gameplay_GS(
                                lobby: lobby,
                                questions: randomizedQuestions,
                                answerTimeDuration: TimeSpan.FromSeconds(answerTimerLength * randomizedQuestions.Count));
                        }
                        else
                        {
                            if (!outlierExtraRound)
                            {
                                return null;
                            }
                            List<State> extraRoundStateChain = new List<State>();
                            List<Question> questionsToRemove = new List<Question>();
                            foreach (Question question in randomizedQuestions)
                            {
                                User differentUser = null;
                                int numAnswersChosen = 0;
                                int numberOfAbstains = 0;
                                List<int> answerGroups = new List<int>();
                                for (int i = 0; i < Question.AnswerTypeToStrings[question.AnswerType].Count; i++)
                                {
                                    answerGroups.Add(0);
                                }
                                foreach (User user in question.UsersToAnswers.Keys)
                                {
                                    if (question.UsersToAnswers[user] == 0)
                                    {
                                        numberOfAbstains++;
                                    }
                                    else if (answerGroups[question.UsersToAnswers[user]] == 0)
                                    {
                                        if(numAnswersChosen == 1)
                                        {
                                            differentUser = user;
                                        }
                                        else if(numAnswersChosen > 1 || differentUser != null)
                                        {
                                            differentUser = null;
                                        }
                                        numAnswersChosen++;
                                    }
                                    
                                    answerGroups[question.UsersToAnswers[user]] = answerGroups[question.UsersToAnswers[user]] + 1;
                                }
                                
                                // checks if the number of outliers is within the threshold and that adding the extra round wouldnt remove too many questions from the end vote
                                if (differentUser != null && (randomizedQuestions.Count - questionsToRemove.Count) > minQuestions && numberOfAbstains < (int)(lobby.GetAllUsers().Count * FriendQuizConstants.ExtraRoundAbstainPercentLimit))
                                {
                                    List<User> randomizedUsers = lobby.GetAllUsers().OrderBy(_ => Rand.Next()).ToList();
                                    questionsToRemove.Add(question);
                                    StateChain extraRoundVotingChain = new StateChain(
                                        stateGenerator: (int counter) =>
                                        {
                                            if (counter == 0)
                                            {
                                                return new ExtraRound_GS(
                                                    lobby: lobby,
                                                    differentUser: differentUser,
                                                    question: question,
                                                    randomizedUsers: randomizedUsers);
                                            }
                                            else if (counter == 1)
                                            {
                                                return new ExtraRoundVoteReveal_GS(
                                                    lobby: lobby,
                                                    differentUser: differentUser,
                                                    question: question,
                                                    randomizedUsers: randomizedUsers);
                                            }
                                            else
                                            {
                                                return null;
                                            }
                                        });
                                    extraRoundStateChain.Add(extraRoundVotingChain);
                                }
                            }
                            foreach (Question question in questionsToRemove)
                            {
                                randomizedQuestions.Remove(question);
                            }
                            if(extraRoundStateChain.Count == 0)
                            {
                                return null;
                            }
                            else
                            {
                                return new StateChain(states: extraRoundStateChain);
                            }
                        }
                    });
                
            }
        }

        public void ValidateOptions(List<ConfigureLobbyRequest.GameModeOptionRequest> gameModeOptions)
        {
           if ((int)gameModeOptions[(int)GameModeOptionsEnum.MaxQuestions].ValueParsed < (int)gameModeOptions[(int)GameModeOptionsEnum.MinQuestions].ValueParsed)
           {
                throw new GameModeInstantiationException("Max cannot be less than min");
           }
        }
    }
}
