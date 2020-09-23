using RoystonGame.TV.ControlFlows.Exit;
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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static System.FormattableString;

namespace RoystonGame.TV.GameModes.TimsGames.FriendQuiz.GameStates
{
    public class Setup_GS: GameState
    {
        private Random Rand { get; } = new Random();
        public Setup_GS(Lobby lobby, RoundTracker roundTracker, TimeSpan? writingDuration = null) : base(lobby)
        {
            List<string> answerTypeStrings = new List<string>();
            foreach(Question.AnswerTypes answerType in Enum.GetValues(typeof(Question.AnswerTypes)))
            {
                string answerTypeString = "";
                foreach(string answer in Question.AnswerTypeToStrings[answerType].Where((string ans) => ans != "Abstain"))
                {
                    answerTypeString = " " + answerTypeString + answer + ",";
                }
                answerTypeStrings.Add(answerTypeString.Trim(','));
            }
            SimplePromptUserState writingUserState = new SimplePromptUserState(
                promptGenerator: (User user) => new UserPrompt()
                {
                    UserPromptId = UserPromptId.FriendQuiz_CreateQuestion,
                    Title = "Let's Make Some Questions",
                    Description = "Write a question and choose an answer type for it",
                    SubPrompts = new SubPrompt[]
                    {
                        new SubPrompt
                        {
                            ShortAnswer = true
                        },
                        new SubPrompt
                        {
                            Answers = answerTypeStrings.ToArray()
                        }
                    },
                    SubmitButton = true
                },
                formSubmitHandler: (User user, UserFormSubmission input) =>
                {                  
                    Question question = new Question()
                    {
                        Owner = user,
                        Text = input.SubForms[0].ShortAnswer,
                        AnswerType = (Question.AnswerTypes)(input.SubForms[1].RadioAnswer ?? 0)
                    };
                    roundTracker.Questions.Add(question);
                    return (true, string.Empty);
                },
                exit: new WaitForUsers_StateExit(
                    lobby: lobby,
                    usersToWaitFor: WaitForUsersType.All),
                maxPromptDuration: writingDuration);
            this.Entrance.Transition(writingUserState);
            writingUserState.Transition(this.Exit);

            this.UnityView = new UnityView(this.Lobby)
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.WaitForUserInputs },
                Title = new StaticAccessor<string> { Value = "Time To Write" },
                Instructions = new StaticAccessor<string> { Value = "Write your questions" },
            };
        }
    }
}
