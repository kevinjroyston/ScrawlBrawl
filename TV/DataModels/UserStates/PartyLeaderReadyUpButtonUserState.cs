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
    /// Gives the player a button they can press which will unblock the flow.
    /// </summary>
    public class PartyLeaderReadyUpButtonUserState : UserState
    {
        public static UserState DefaultState(PartyLeaderReadyUpButtonUserState state = null) => state ?? new PartyLeaderReadyUpButtonUserState();

        public static UserPrompt DefaultPrompt(UserPrompt prompt = null) => prompt ?? new UserPrompt() { Title = "You have the power!", Description = "Click submit when everybody is ready :)", RefreshTimeInMs = 5000, SubmitButton = true };

        public PartyLeaderReadyUpButtonUserState(UserPrompt prompt = null) : base(null, null, DefaultPrompt(prompt))
        {
            // Empty
        }

        public override bool HandleUserFormInput(User user, UserFormSubmission userInput)
        {
            // No validation needed
            this.Outlet(user, UserStateResult.Success, null);
            return true;
        }
    }
}
