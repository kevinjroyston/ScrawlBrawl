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
    /// On state completion, transitions users to a waiting state until a trigger occurs.
    /// </summary>
    public class NoWait : UserStateTransition
    {
        // Literally only needed to satisfy the new() constraint needed by StateExtensions.cs
        public NoWait() : this(null) { }

        public NoWait(Action<User, UserStateResult, UserFormSubmission> outlet = null) : base(outlet)
        {
            // Empty
        }

        public override void Inlet(User user, UserStateResult stateResult, UserFormSubmission formSubmission)
        {
            this.Outlet(user, stateResult, formSubmission);
        }
    }
}
