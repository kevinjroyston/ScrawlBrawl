using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;

namespace RoystonGame.TV.DataModels.UserStates
{
    /// <summary>
    /// A waiting state which will display "prompt" until state is forcefully changed occurs.
    /// </summary>
    public class WaitingUserState : UserState
    {
        /// <summary>
        /// Helper function for other classes to easily get a default WaitingUserState.
        /// </summary>
        /// <param name="state">To be returned, or overriden with default if null.</param>
        /// <returns>The default waiting user state if none provided.</returns>
        public static WaitingUserState DefaultState() => new WaitingUserState();
        public static UserPrompt DefaultPrompt() => new UserPrompt() { Description = "Waiting . . .", RefreshTimeInMs = 1000};

        public WaitingUserState(UserPrompt prompt = null, TimeSpan? maxWaitTime = null) : base(null, maxWaitTime, prompt ?? DefaultPrompt())
        {
            // Empty
        }

        public override bool HandleUserFormInput(User user, UserFormSubmission userInput)
        {
            return false;
        }
    }
}
