using RoystonGame.TV.DataModels;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.TV.DataModels.UserStates;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Connector = System.Action<
    RoystonGame.TV.DataModels.User,
    RoystonGame.TV.DataModels.Enums.UserStateResult,
    RoystonGame.Web.DataModels.Requests.UserFormSubmission>;
namespace RoystonGame.TV.ControlFlows
{
    public class SimplePromptAndWaitForUsers : WaitForAllPlayers
    {
        private UserState PromptedPlayersState { get; }
        private List<User> PromptedPlayers { get; }
        public SimplePromptAndWaitForUsers(
            Lobby lobby,
            Connector outlet = null,
            List<User> promptedPlayers = null,
            Func<User, UserPrompt> promptedPlayersPrompt = null,
            Func<User, UserPrompt> waitingPrompt = null,
            Func<User, UserFormSubmission, (bool, string)> formSubmitListener = null) : base(lobby, null, outlet, StateHelpers.CreateWaitingUserStateFromPromptGenerator(waitingPrompt))

        {
            this.PromptedPlayers = promptedPlayers;
            this.PromptedPlayersState = new SimplePromptUserState(prompt: promptedPlayersPrompt, formSubmitListener: formSubmitListener) ?? PartyLeaderReadyUpButtonUserState.DefaultState();
            this.PromptedPlayersState.SetOutlet(base.Inlet);
        }

        public override void Inlet(User user, UserStateResult stateResult, UserFormSubmission formSubmission)
        {
            if (PromptedPlayers.Contains(user))
            {
                this.PromptedPlayersState.Inlet(user, stateResult, formSubmission);
            }
            else
            {
                base.Inlet(user, stateResult, formSubmission);
            }
        }
    }
}
