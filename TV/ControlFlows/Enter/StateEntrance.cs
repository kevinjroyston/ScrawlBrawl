using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RoystonGame.TV.DataModels;
using RoystonGame.TV.ControlFlows.Exit;

namespace RoystonGame.TV.ControlFlows.Enter
{
    public class StateEntrance : StateOutlet
    {
        public StateEntrance(StateExit stateExit = null): base(stateExit)
        {

        }
        public void Inlet(User user, UserStateResult stateResult, UserFormSubmission formSubmission)
        {
            this.Exit.Inlet(user, stateResult, formSubmission);
        }
    }
}
