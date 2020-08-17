using RoystonGame.TV.ControlFlows.Exit;
using RoystonGame.TV.DataModels;
using RoystonGame.TV.DataModels.States.GameStates;
using RoystonGame.TV.DataModels.States.StateGroups;
using RoystonGame.TV.DataModels.States.UserStates;
using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.Extensions;
using RoystonGame.TV.GameModes.BriansGames.BattleReady.DataModels;
using RoystonGame.TV.GameModes.TimsGames.FriendQuiz.DataModels;
using RoystonGame.Web.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
using RoystonGame.Web.DataModels.UnityObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.TV.GameModes.TimsGames.FriendQuiz.GameStates
{
    public class Gameplay_GS : GameState
    {
        public Gameplay_GS(Lobby lobby, List<Question> questions, TimeSpan? answerTimeDuration = null) : base(lobby)
        {
            List<State> GetAsnwerUserStateChain(User user)
            {
                List<State> stateChain = new List<State>();
                foreach (Question question in questions)
                {
                    stateChain.Add(new SimplePromptUserState(
                        promptGenerator: (User user) => new UserPrompt()
                        {
                            Title = "Answer this question as truthfully as you can",
                            Description = question.Text,
                            SubPrompts = new SubPrompt[]
                            {
                                new SubPrompt
                                {
                                    Answers = Question.AnswerTypeToStrings[question.AnswerType].ToArray()
                                }
                            },
                            SubmitButton = true
                        },
                        formSubmitHandler: (User user, UserFormSubmission input) =>
                        {
                            question.UsersToAnswers.TryAdd(user, input.SubForms[0].RadioAnswer ?? 0);
                            return (true, string.Empty);
                        }));
                }
                return stateChain;
            }
            MultiStateChain askQuestions = new MultiStateChain(GetAsnwerUserStateChain, exit: new WaitForUsers_StateExit(lobby));

            this.Entrance.Transition(askQuestions);
            askQuestions.Transition(this.Exit);

            this.UnityView = new UnityView(lobby)
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.WaitForUserInputs },
                Instructions = new StaticAccessor<string> { Value = "Answer all the questions on your phones" },
            };
        }
    }
}
