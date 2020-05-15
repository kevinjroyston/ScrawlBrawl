using RoystonGame.TV.DataModels;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
