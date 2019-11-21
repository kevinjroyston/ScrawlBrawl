using RoystonGame.TV.DataModels;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.TV.DataModels.UserStates;
using RoystonGame.Web.DataModels.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.TV.ControlFlows
{
    /// <summary>
    /// Responsible for transitioning users from one state to another.
    /// </summary>
    public abstract class UserStateTransition : State
    {
        public UserStateTransition(Action<User, UserStateResult, UserFormSubmission> outlet = null)
        {
            if (outlet != null)
            {
                this.SetOutlet(outlet);
            }
        }
    }
}
