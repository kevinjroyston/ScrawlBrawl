using RoystonGame.TV.DataModels;
using RoystonGame.TV.DataModels.UserStates;
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
    public static class StateHelpers
    {
        /*public static SimplePromptUserState CreateSimplePromptUserStateFromPromptGenerator(Func<User, UserPrompt> promptGenerator, Connector outlet =null )
        {
            return new SimplePromptUserState(outlet: outlet, prompt: promptGenerator);
        }*/
        public static WaitingUserState CreateWaitingUserStateFromPromptGenerator(Func<User, UserPrompt> promptGenerator, Connector outlet = null)
        {
            if(promptGenerator==null)
            {
                WaitingUserState toReturn = WaitingUserState.DefaultState();
                toReturn.SetOutlet(outlet);
                return (WaitingUserState)toReturn;
            }
            return new WaitingUserState(promptGenerator: promptGenerator, outlet: outlet);
        }
    }
}
