using RoystonGame.TV.DataModels;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.TV.DataModels.UserStates;
using RoystonGame.Web.DataModels.Requests;
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
    /// <summary>
    /// Responsible for transitioning users from one state to another.
    /// </summary>
    public abstract class UserStateTransition : State
    {
        public UserStateTransition(Connector outlet = null)
        {
            if (outlet != null)
            {
                this.SetOutlet(outlet);
            }
        }
    }
}
