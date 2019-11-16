using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RoystonGame.Game.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;

namespace RoystonGame.Game.DataModels.UserStates
{
    /// <summary>
    /// A waiting state which will display "prompt" forever until state is forcefully changed or the prompt is submitted.
    /// </summary>
    public class SimplePromptUserState : UserState
    {
        public static UserPrompt DefaultPrompt(UserPrompt prompt) => prompt ?? new UserPrompt() { Description = "Waiting . . .", RefreshTimeInMs = 1000 };

        public SimplePromptUserState(UserPrompt prompt = null, TimeSpan? maxPromptDuration = null)
            : base(null, maxPromptDuration ?? TimeSpan.MaxValue, DefaultPrompt(prompt))
        {
            // Empty
        }

        public override bool HandleUserFormInput(User user, UserFormSubmission userInput)
        {
            // TODO validate userInput
            this.StateCompletedCallback(user, UserStateResult.Success, userInput);
            return true;
        }
    }
}
