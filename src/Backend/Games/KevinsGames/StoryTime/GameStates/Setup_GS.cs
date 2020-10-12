using Backend.GameInfrastructure.ControlFlows.Exit;
using Backend.GameInfrastructure.DataModels.States.GameStates;
using Backend.GameInfrastructure.DataModels.States.UserStates;
using Backend.GameInfrastructure.DataModels.Users;
using Backend.GameInfrastructure.Extensions;
using Backend.Games.KevinsGames.StoryTime.DataModels;
using Common.DataModels.Requests;
using Common.DataModels.Responses;
using Backend.APIs.DataModels.UnityObjects;
using System;
using static Backend.Games.KevinsGames.StoryTime.DataModels.RoundTracker;
using static System.FormattableString;
using Backend.GameInfrastructure;
using Common.DataModels.Enums;
using Common.Code.Extensions;

namespace Backend.Games.KevinsGames.StoryTime.GameStates
{
    public class Setup_GS : GameState
    {
        private Random Rand { get; } = new Random();
        public Setup_GS(Lobby lobby, string prompt, RoundTracker roundTracker, TimeSpan? writingDuration = null) : base(lobby)
        {
            SimplePromptUserState writingUserState = new SimplePromptUserState(
                promptGenerator: (User user) => new UserPrompt()
                {
                    UserPromptId = UserPromptId.StoryTime_StartPromptChain,
                    Title = "Time To Write",
                    Description = Invariant($"Write the first sentence for a \"{prompt}\""),
                    SubPrompts = new SubPrompt[]
                    {
                        new SubPrompt
                        {
                            ShortAnswer = true
                        }
                    },
                    SubmitButton = true
                },
                formSubmitHandler: (User user, UserFormSubmission input) =>
                {
                    string text = input.SubForms[0].ShortAnswer;
                    UserWriting writing = new UserWriting(user, text, WritingDisplayPosition.None);
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
                Instructions = new StaticAccessor<string> { Value = Invariant($"Write the first sentence for a \"{prompt}\"") },
            };
        }
    }
}
