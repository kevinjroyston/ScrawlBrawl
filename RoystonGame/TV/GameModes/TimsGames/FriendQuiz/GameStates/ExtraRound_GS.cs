using RoystonGame.TV.ControlFlows.Exit;
using RoystonGame.TV.DataModels;
using RoystonGame.TV.DataModels.States.GameStates;
using RoystonGame.TV.DataModels.States.UserStates;
using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.Extensions;
using RoystonGame.TV.GameModes.TimsGames.FriendQuiz.DataModels;
using RoystonGame.Web.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
using RoystonGame.Web.DataModels.UnityObjects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static System.FormattableString;

namespace RoystonGame.TV.GameModes.TimsGames.FriendQuiz.GameStates
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
                    user.AddScore(FriendQuizConstants.PointsForExtraRound);
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
            this.UnityView = new UnityView(lobby)
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.TextView },
                Title = new StaticAccessor<string> { Value = "Oh! It looks like we have an outlier." },
                Instructions = new StaticAccessor<string> { Value = instructions },
            };
        }
    }
}
