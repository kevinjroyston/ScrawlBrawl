using RoystonGame.Game.DataModels;
using RoystonGame.Game.DataModels.Enums;
using RoystonGame.Game.DataModels.UserStates;
using RoystonGame.Web.DataModels.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.Game.ControlFlows
{
    /// <summary>
    /// On state completion, transitions users to a waiting state until a trigger occurs.
    /// </summary>
    public class NoWait_UST : UserStateTransition
    {
        public NoWait_UST(Action<User, UserStateResult, UserFormSubmission> outlet) : base(outlet)
        {
            // Empty
        }

        public override void AddUsersToTransition(IEnumerable<User> users)
        {
            // Empty
        }

        public override void Inlet(User user, UserStateResult stateResult, UserFormSubmission formSubmission)
        {
            this.Outlet(user, stateResult, formSubmission);
        }
    }
}
