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
        public static UserPrompt DefaultPrompt(User user) => new UserPrompt() { Description = "Waiting . . .", RefreshTimeInMs = 1000};

        public WaitingUserState(Func<User, UserPrompt> promptGenerator = null, TimeSpan? maxWaitTime = null) : base(null, maxWaitTime, promptGenerator ?? DefaultPrompt)
        {
            // Empty
        }

        public override bool HandleUserFormInput(User user, UserFormSubmission userInput, out string error)
        {
            error = "You shouldn't be submitting right now. Go away >:(";
            return false;
        }
    }
}
