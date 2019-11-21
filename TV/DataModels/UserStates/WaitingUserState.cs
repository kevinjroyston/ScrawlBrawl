using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;

namespace RoystonGame.TV.DataModels.UserStates
{
    /// <summary>
    /// A waiting state which will display "prompt" forever until state is forcefully changed.
    /// </summary>
    public class WaitingUserState : UserState
    {
        /// <summary>
        /// Helper function for other classes to easily get a default WaitingUserState.
        /// </summary>
        /// <param name="state">To be returned, or overriden with default if null.</param>
        /// <returns>The default waiting user state if none provided.</returns>
        public static WaitingUserState DefaultState(WaitingUserState state) => state ?? new WaitingUserState();
        public static UserPrompt DefaultPrompt(UserPrompt prompt) => prompt ?? new UserPrompt() { Description = "Waiting . . .", RefreshTimeInMs = 1000};

        public WaitingUserState(UserPrompt prompt = null) : base(null, null, DefaultPrompt(prompt))
        {
            // Empty
        }

        public override bool HandleUserFormInput(User user, UserFormSubmission userInput)
        {
            // If one of you triggers this im going to bop you on the head.
            throw new Exception("User should not be inputting while waiting");
        }
    }
}
