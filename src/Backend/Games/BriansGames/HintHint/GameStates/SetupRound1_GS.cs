using Backend.APIs.DataModels.UnityObjects;
using Backend.GameInfrastructure;
using Backend.GameInfrastructure.ControlFlows.Exit;
using Backend.GameInfrastructure.DataModels.Enums;
using Backend.GameInfrastructure.DataModels.States.GameStates;
using Backend.GameInfrastructure.DataModels.States.UserStates;
using Backend.GameInfrastructure.DataModels.Users;
using Backend.GameInfrastructure.Extensions;
using Backend.Games.BriansGames.HintHint.DataModels;
using Common.DataModels.Enums;
using Common.DataModels.Requests;
using Common.DataModels.Responses;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static System.FormattableString;

namespace Backend.Games.BriansGames.HintHint.GameStates
{
    public class SetupRound1_GS : GameState
    {
        public SetupRound1_GS(Lobby lobby, ConcurrentBag<RealFakePair> realFakePairs, TimeSpan? setupDuration)
            : base(
                  lobby: lobby,
                  exit: new WaitForUsers_StateExit(lobby))
        {
            UserState getRealPrompt = new SimplePromptUserState(
                promptGenerator: (User user) => new UserPrompt()
                {
                    UserPromptId = UserPromptId.HintHint_SetupRound1,
                    Title = "Game Setup Round 1",
                    Description = "Enter a word for people to guess (You will either be giving the real of fake hints for what you put)",
                    SubPrompts = new SubPrompt[]
                    {
                        new SubPrompt
                        {
                            Prompt = Invariant($"The word for others to guess"),
                            ShortAnswer = true,
                        },
                    },
                    SubmitButton = true
                },
                formSubmitHandler: (User user, UserFormSubmission input) =>
                {
                    RealFakePair rfp = new RealFakePair() { RealGoal = input.SubForms[0].ShortAnswer, Creator = user };
                    rfp.BannedMemberIds.Add(user.Id);
                    realFakePairs.Add(rfp);
                    return (true, string.Empty);
                },
                userTimeoutHandler: (User user, UserFormSubmission input) =>
                {
                    if (input.SubForms?.Count > 0)
                    {
                        if (input.SubForms[0]?.ShortAnswer != null)
                        {
                            realFakePairs.Add(new RealFakePair() { RealGoal = input.SubForms[0].ShortAnswer });
                        }
                    }
                    return UserTimeoutAction.None;
                },
                exit: new WaitForUsers_StateExit(lobby: this.Lobby, usersToWaitFor: WaitForUsersType.All),
                maxPromptDuration: setupDuration);

            this.Entrance.Transition(getRealPrompt);
            getRealPrompt.Transition(this.Exit);

            this.UnityView = new UnityView(this.Lobby)
            {
                ScreenId = TVScreenId.WaitForUserInputs,
                Instructions = new UnityField<string> { Value = "On your devices come up with a word for people to guess" },
            };
        }
    }
}
