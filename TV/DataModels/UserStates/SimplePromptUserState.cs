using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;

using Connector = System.Action<
    RoystonGame.TV.DataModels.User,
    RoystonGame.TV.DataModels.Enums.UserStateResult,
    RoystonGame.Web.DataModels.Requests.UserFormSubmission>;

namespace RoystonGame.TV.DataModels.UserStates
{
    /// <summary>
    /// A waiting state which will display "prompt" forever until state is forcefully changed or the prompt is submitted.
    /// </summary>
    public class SimplePromptUserState : UserState
    {
        public static UserPrompt DefaultPrompt(User user) => new UserPrompt() { Description = "Waiting . . .", RefreshTimeInMs = 1000 };
        private List<Func<User, UserFormSubmission, bool>> FormSubmitListeners { get; set; } = new List<Func<User, UserFormSubmission, bool>>();
        public SimplePromptUserState(Func<User, UserPrompt> prompt = null, Connector outlet = null, TimeSpan? maxPromptDuration = null, Func<User, UserFormSubmission, bool> formSubmitListener = null)
            : base(outlet, maxPromptDuration, prompt ?? DefaultPrompt)
        {
            if (formSubmitListener!= null)
            {
                FormSubmitListeners.Add(formSubmitListener);
            }
        }

        public void AddFormSubmitListener(Func<User, UserFormSubmission, bool> listener)
        {
            FormSubmitListeners.Add(listener);
        }

        public override bool HandleUserFormInput(User user, UserFormSubmission userInput)
        {
            if (!base.HandleUserFormInput(user, userInput))
            {
                return false;
            }

            bool success = true;
            foreach(var listener in this.FormSubmitListeners)
            {
                try
                {
                    success &= listener?.Invoke(user, userInput) ?? true;
                }
                catch
                {
                    return false;
                }
            }

            if (!success)
            {
                return false;
            }

            this.Outlet(user, UserStateResult.Success, userInput);
            return true;
        }
    }
}
