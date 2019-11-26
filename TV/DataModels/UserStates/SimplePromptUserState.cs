using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;

namespace RoystonGame.TV.DataModels.UserStates
{
    /// <summary>
    /// A waiting state which will display "prompt" forever until state is forcefully changed or the prompt is submitted.
    /// </summary>
    public class SimplePromptUserState : UserState
    {
        public static UserPrompt DefaultPrompt(UserPrompt prompt) => prompt ?? new UserPrompt() { Description = "Waiting . . .", RefreshTimeInMs = 1000 };
        private Action<User, UserFormSubmission> FormSubmitCallback { get; set; }
        public SimplePromptUserState(UserPrompt prompt = null, Action<User, UserStateResult, UserFormSubmission> outlet = null, TimeSpan? maxPromptDuration = null, Action<User, UserFormSubmission> formSubmitCallback = null)
            : base(outlet, maxPromptDuration, DefaultPrompt(prompt))
        {
            this.FormSubmitCallback = formSubmitCallback;
        }

        public override bool HandleUserFormInput(User user, UserFormSubmission userInput)
        {
            if (!base.HandleUserFormInput(user, userInput))
            {
                return false;
            }

            this.FormSubmitCallback?.Invoke(user, userInput);
            this.Outlet(user, UserStateResult.Success, userInput);
            return true;
        }
    }
}
