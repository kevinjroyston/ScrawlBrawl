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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static System.FormattableString;

namespace RoystonGame.TV.GameModes.TimsGames.FriendQuiz.GameStates
{
    public class Voting_GS : GameState
    {
        private Random Rand { get; } = new Random();
        public Voting_GS(Lobby lobby, List<Question> questionsToShow, User userToShow, TimeSpan? votingTime) : base(lobby)
        {
            ConcurrentDictionary<Question, ConcurrentDictionary<User, int>> questionsToUserAnswers = new ConcurrentDictionary<Question, ConcurrentDictionary<User, int>>();
      
            SelectivePromptUserState guessAnswers = new SelectivePromptUserState(
                usersToPrompt: lobby.GetAllUsers().Where((User user) => user != userToShow).ToList(),
                promptGenerator: (User user) => new UserPrompt()
                {
                    Title = userToShow.DisplayName,
                    Description = Invariant($"How do you think {userToShow.DisplayName} answered these questions?"),
                    SubPrompts = questionsToShow.Select((Question question) =>
                    {
                        return new SubPrompt()
                        {
                            Prompt = question.Text,
                            Answers = Question.AnswerTypeToStrings[question.AnswerType].Where((string ans) => ans != "Abstain").ToArray()
                        };
                    }).ToArray(),
                    SubmitButton = true
                },
                formSubmitHandler: (User user, UserFormSubmission input) =>
                {
                    for (int i = 0; i < input.SubForms.Count; i++)
                    {
                        if (!questionsToUserAnswers.ContainsKey(questionsToShow[i]))
                        {
                            questionsToUserAnswers.TryAdd(questionsToShow[i], new ConcurrentDictionary<User, int>());
                        }
                        questionsToUserAnswers[questionsToShow[i]].TryAdd(user, (input.SubForms[i].RadioAnswer ?? -1) + 1);
                    }
                    return (true, string.Empty);
                },
                exit: new WaitForUsers_StateExit(
                    lobby: lobby,
                    usersToWaitFor: WaitForUsersType.All),
                maxPromptDuration: votingTime);

            guessAnswers.AddExitListener(() =>
            {
                foreach (Question question in questionsToShow)
                {
                    if (questionsToUserAnswers.ContainsKey(question))
                    {
                        foreach (User user in lobby.GetAllUsers())
                        {
                            if (questionsToUserAnswers[question].ContainsKey(user))
                            {
                                if (questionsToUserAnswers[question][user] == question.UsersToAnswers[userToShow])
                                {
                                    user.Score += FriendQuizConstants.PointsForCorrectAnswer;
                                }
                            }
                        }
                    }
                }
            });

            List<UnityImage> displayTexts = questionsToShow.Select((Question question) =>
            {
                string title = "<b>" + question.Text + "</b> ";
                List<string> answers = Question.AnswerTypeToStrings[question.AnswerType].Where((string ans) => ans != "Abstain").ToList();

                string formattedText = string.Join(" | ", answers);

                return new UnityImage()
                {
                    Title = new StaticAccessor<string> { Value = title },
                    Header = new StaticAccessor<string> { Value = formattedText }
                };
            }).ToList();
            this.Entrance.Transition(guessAnswers);
            guessAnswers.Transition(this.Exit);
            this.UnityView = new UnityView(this.Lobby)
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.TextView },
                Title = new StaticAccessor<string> { Value = Invariant($"How do you think {userToShow.DisplayName} answered these questions?")},
                UnityImages = new StaticAccessor<IReadOnlyList<UnityImage>> { Value = displayTexts},
                Options = new StaticAccessor<UnityViewOptions>
                {
                    Value = new UnityViewOptions()
                    {
                        PrimaryAxis = new StaticAccessor<Axis?> { Value = Axis.Vertical },
                        PrimaryAxisMaxCount = new StaticAccessor<int?> { Value = 8 }
                    }
                }
            };
                       
        }
    }
}
