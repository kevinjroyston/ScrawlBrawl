using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.DataModels.States.GameStates;
using RoystonGame.TV.DataModels.States.UserStates;
using RoystonGame.TV.Extensions;
using RoystonGame.TV.GameModes.Common;
using RoystonGame.Web.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Requests.LobbyManagement;
using RoystonGame.Web.DataModels.Responses;
using RoystonGame.Web.DataModels.UnityObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using RoystonGame.TV.DataModels;
using RoystonGame.TV.ControlFlows.Exit;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.TV.GameModes.Common.DataModels;
using System.Collections.Concurrent;
using RoystonGame.TV.GameModes.KevinsGames.StoryTime.DataModels;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.IdentityModel.Tokens;
using static System.FormattableString;
using static RoystonGame.TV.GameModes.KevinsGames.StoryTime.DataModels.RoundTracker;

namespace RoystonGame.TV.GameModes.KevinsGames.StoryTime.GameStates
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
