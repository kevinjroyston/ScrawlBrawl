using Backend.GameInfrastructure.ControlFlows.Exit;
using Backend.GameInfrastructure.DataModels;
using Backend.GameInfrastructure.DataModels.States.GameStates;
using Backend.GameInfrastructure.DataModels.States.UserStates;
using Backend.GameInfrastructure.DataModels.Users;
using Backend.GameInfrastructure.Extensions;
using Backend.Games.TimsGames.FriendQuiz.DataModels;
using Common.DataModels.Requests;
using Common.DataModels.Responses;
using Backend.APIs.DataModels.UnityObjects;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Common.DataModels.Enums;
using Backend.GameInfrastructure;

namespace Backend.Games.TimsGames.FriendQuiz.GameStates
{
    public class ExtraRound_GS : GameState
    {
        public ExtraRound_GS(Lobby lobby, User differentUser, Question question, List<User> randomizedUsers) : base(lobby: lobby, exit: new WaitForUsers_StateExit(lobby))
        {
            ConcurrentDictionary<User, User> userToVoteResults = new ConcurrentDictionary<User, User>();
            State votingState = new SelectivePromptUserState(
                usersToPrompt: lobby.GetAllUsers().Where((User user) => !user.Equals(differentUser)).ToList(),
                promptGenerator: (User user) => new UserPrompt()
                {
                    UserPromptId = UserPromptId.FriendQuiz_ExtraRoundVoting,
                    Title = "Who do you think answered this question differently?",
                    Description = question.Text,
                    SubPrompts = new SubPrompt[]
                    {
                        new SubPrompt
                        {
                            Answers = randomizedUsers.Select((User user) => user.DisplayName).ToArray()
                        }
                    },
                    SubmitButton = true
                },
                formSubmitHandler: (User user, UserFormSubmission input) =>
                {
                    if (input.SubForms[0].RadioAnswer != null)
                    {
                        userToVoteResults.TryAdd(user, randomizedUsers[(int)input.SubForms[0].RadioAnswer]);
                    }                         
                    return (true, string.Empty);
                });
            votingState.AddExitListener(() =>
            {
            foreach (User user in userToVoteResults.Keys)
            {
                if (differentUser.Equals(userToVoteResults[user]))
                {
                    user.ScoreHolder.AddScore(FriendQuizConstants.PointsForExtraRound, Score.Reason.CorrectAnswer);
                }

                question.ExtraRoundUserToVotesRecieved.AddOrUpdate(
                    key: userToVoteResults[user],
                    addValue: 1,
                    updateValueFactory: (User user, int value) => value + 1);
                }
            });

            this.Entrance.Transition(votingState);
            votingState.Transition(this.Exit);

            string majorityAnswer = Question.AnswerTypeToStrings[question.AnswerType]
                [question.UsersToAnswers[randomizedUsers.Where((User user) => !differentUser.Equals(user) && question.UsersToAnswers[user] != 0).ToList()[0]]];
            string outlierAnswer = Question.AnswerTypeToStrings[question.AnswerType]
                [question.UsersToAnswers[differentUser]];

            string instructions = question.Text + ":    " + (question.UsersToAnswers.Count - 1) + " players put " + majorityAnswer + " or Abstained, 1 player put " + outlierAnswer;
            this.Legacy_UnityView = new Legacy_UnityView(lobby)
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.TextView },
                Title = new StaticAccessor<string> { Value = "Oh! It looks like we have an outlier." },
                Instructions = new StaticAccessor<string> { Value = instructions },
            };
        }
    }
}
