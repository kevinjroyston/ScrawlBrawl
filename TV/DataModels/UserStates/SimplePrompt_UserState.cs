<<<<<<< HEAD:TV/DataModels/States/UserStates/SimplePromptUserState.cs
﻿using RoystonGame.TV.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using static System.FormattableString;
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
        public static UserPrompt DefaultPrompt(User user) => new UserPrompt() { Description = "Waiting . . ." };
        private List<Func<User, UserFormSubmission, (bool, string)>> FormSubmitListeners { get; set; } = new List<Func<User, UserFormSubmission, (bool, string)>>();
        public SimplePromptUserState(Func<User, UserPrompt> prompt = null, Connector outlet = null, TimeSpan? maxPromptDuration = null, Func<User, UserFormSubmission, (bool, string)> formSubmitListener = null)
            : base(outlet, maxPromptDuration, prompt ?? DefaultPrompt)
        {
            if (formSubmitListener != null)
            {
                FormSubmitListeners.Add(formSubmitListener);
            }
        }

        public void AddFormSubmitListener(Func<User, UserFormSubmission, (bool, string)> listener)
        {
            FormSubmitListeners.Add(listener);
        }

        public override bool HandleUserFormInput(User user, UserFormSubmission userInput, out string error)
        {
            if (!base.HandleUserFormInput(user, userInput, out error))
            {
                return false;
            }

            bool success = true;
            foreach (var listener in this.FormSubmitListeners)
            {
                try
                {
                    var successTuple = listener?.Invoke(user, userInput) ?? null;
                    error = !string.IsNullOrWhiteSpace(error) ? error : (successTuple?.Item2 ?? string.Empty);
                    success &= successTuple?.Item1 ?? true;
                }
                catch(Exception exception)
                {
                    Debug.WriteLine(exception);
                    Debug.Assert(false, exception.ToString());
                    error = "Something went wrong.";
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
=======
﻿using RoystonGame.TV.ControlFlows.Enter;
using RoystonGame.TV.ControlFlows.Exit;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
using System;
using System.Collections.Generic;
using Connector = System.Action<
    RoystonGame.TV.DataModels.User,
    RoystonGame.TV.DataModels.Enums.UserStateResult,
    RoystonGame.Web.DataModels.Requests.UserFormSubmission>;

namespace RoystonGame.TV.DataModels.UserStates
{
    /// <summary>
    /// A waiting state which will display "prompt" forever until state is forcefully changed or the prompt is submitted.
    /// </summary>
    public class SimplePrompt_UserState : UserState
    {
        public static UserPrompt DefaultPrompt(User user) => new UserPrompt() { Description = "Waiting . . ." };
        private List<Func<User, UserFormSubmission, (bool, string)>> FormSubmitListeners { get; set; } = new List<Func<User, UserFormSubmission, (bool, string)>>();
        public SimplePrompt_UserState(Func<User, UserPrompt> prompt = null, TimeSpan? maxPromptDuration = null, Func<User, UserFormSubmission, (bool, string)> formSubmitListener = null, StateEntrance entrance = null, StateExit exit = null)
            : base(maxPromptDuration, prompt ?? DefaultPrompt, entrance: entrance, exit: exit)
        {
            if (formSubmitListener != null)
            {
                FormSubmitListeners.Add(formSubmitListener);
            }
        }

        public void AddFormSubmitListener(Func<User, UserFormSubmission, (bool, string)> listener)
        {
            FormSubmitListeners.Add(listener);
        }

        public override bool HandleUserFormInput(User user, UserFormSubmission userInput, out string error)
        {
            if (!base.HandleUserFormInput(user, userInput, out error))
            {
                return false;
            }

            bool success = true;
            foreach (var listener in this.FormSubmitListeners)
            {
                try
                {
                    var successTuple = listener?.Invoke(user, userInput) ?? null;
                    error = !string.IsNullOrWhiteSpace(error) ? error : (successTuple?.Item2 ?? string.Empty);
                    success &= successTuple?.Item1 ?? true;
                }
                catch
                {
                    error = "Something went wrong.";
                    return false;
                }
            }

            if (!success)
            {
                return false;
            }

            this.Exit.Inlet(user, UserStateResult.Success, userInput);
            return true;
        }
    }
}
>>>>>>> commit before rebase:TV/DataModels/UserStates/SimplePrompt_UserState.cs
