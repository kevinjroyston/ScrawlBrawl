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
    /// Gives the player a button they can press which will unblock the flow.
    /// </summary>
    public class FirstPlayerReadyUpButtonUserState : UserState
    {
        public static UserState DefaultState(FirstPlayerReadyUpButtonUserState state = null) => state ?? new FirstPlayerReadyUpButtonUserState();

        public static UserPrompt DefaultPrompt(UserPrompt prompt = null) => prompt ?? new UserPrompt() { Title = "You have the power!", Description = "Click submit when everybody is ready :)", RefreshTimeInMs = 5000 };

        public FirstPlayerReadyUpButtonUserState(UserPrompt prompt = null) : base(null, TimeSpan.MaxValue, DefaultPrompt(prompt))
        {
            // Empty
        }

        public override bool HandleUserFormInput(User user, UserFormSubmission userInput)
        {
            // No validation needed
            this.StateCompletedCallback(user, UserStateResult.Success, null);
            return true;
        }
    }
}
