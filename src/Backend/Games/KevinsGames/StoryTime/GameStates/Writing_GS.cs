using Backend.GameInfrastructure.DataModels.Users;
using Backend.GameInfrastructure.DataModels.States.GameStates;
using Backend.GameInfrastructure.DataModels.States.UserStates;
using Backend.GameInfrastructure.Extensions;
using Common.DataModels.Requests;
using Common.DataModels.Responses;
using Backend.APIs.DataModels.UnityObjects;
using System;
using System.Collections.Generic;
using Backend.GameInfrastructure.ControlFlows.Exit;
using Backend.Games.KevinsGames.StoryTime.DataModels;
using static System.FormattableString;
using static Backend.Games.KevinsGames.StoryTime.DataModels.RoundTracker;
using Backend.GameInfrastructure;
using Common.DataModels.Enums;
using Common.Code.Extensions;

namespace Backend.Games.KevinsGames.StoryTime.GameStates
{
    public class Writing_GS : GameState
    {
        private Random Rand { get; } = new Random();
        public Writing_GS(Lobby lobby, List<User> usersWriting, string prompt, string oldText, RoundTracker roundTracker, TimeSpan? writingDuration = null) : base(lobby)
        {
            SelectivePromptUserState writingUserState = new SelectivePromptUserState(
                usersToPrompt: usersWriting,
                promptGenerator: (User user) => new UserPrompt()
                {
                    UserPromptId = UserPromptId.StoryTime_ContinuePromptChain,
                    Title = "Time To Write",
                    Description = Invariant($"Write a new sentence to put before or after \"{oldText}\" in order to make it the best \"{prompt}\""),
                    SubPrompts = new SubPrompt[]
                    {
                        new SubPrompt
                        {
                            ShortAnswer = true
                        },
                        new SubPrompt
                        {
                            Answers = new string[]
                            {
                                "Place Before",
                                "Place After"
                            }
                        }
                    },
                    SubmitButton = true
                },
                formSubmitHandler: (User user, UserFormSubmission input) =>
                {
                    string text = input.SubForms[0].ShortAnswer;
                    WritingDisplayPosition position = WritingDisplayPosition.After;
                    if(input.SubForms[1].RadioAnswer == 0)
                    {
                        position = WritingDisplayPosition.Before;
                    }
                    UserWriting writing = new UserWriting(user, text, position);
                    roundTracker.UsersToUserWriting.AddOrReplace(user, writing);
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
                Users = new StaticAccessor<IReadOnlyList<User>> { Value = usersWriting },
                Instructions = new StaticAccessor<string> { Value = Invariant($"Write a new sentence to put before or after \"{oldText}\" in order to make it the best \"{prompt}\"") },
            };
        }
    }
}
