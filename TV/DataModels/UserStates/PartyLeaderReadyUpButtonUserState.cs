using RoystonGame.TV.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
using System;

namespace RoystonGame.TV.DataModels.UserStates
{
    /// <summary>
    /// Gives the player a button they can press which will unblock the flow.
    /// </summary>
    public class PartyLeaderReadyUpButtonUserState : UserState
    {
        public static UserState DefaultState() => new PartyLeaderReadyUpButtonUserState();

        public static UserPrompt DefaultPrompt(User user) => new UserPrompt() { Title = "You have the power!", Description = "Click submit when everybody is ready :)", RefreshTimeInMs = 5000, SubmitButton = true };

        public PartyLeaderReadyUpButtonUserState(Func<User, UserPrompt> prompt = null) : base(null, null, prompt ?? DefaultPrompt)
        {
            // Empty
        }

        public override bool HandleUserFormInput(User user, UserFormSubmission userInput, out string error)
        {
            if (!base.HandleUserFormInput(user, userInput, out error))
            {
                return false;
            }

            // No validation needed
            this.Outlet(user, UserStateResult.Success, null);
            return true;
        }
    }
}
