using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;

namespace RoystonGame.Game.DataModels
{
    /// <summary>
    /// A waiting state which will display "prompt" forever until state is forcefully changed.
    /// </summary>
    public class WaitingUserState : UserState
    {
        public WaitingUserState(UserPrompt prompt) : base(null, TimeSpan.MaxValue, prompt)
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
