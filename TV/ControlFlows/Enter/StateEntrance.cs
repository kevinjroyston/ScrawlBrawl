﻿using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.TV.DataModels;
using RoystonGame.TV.ControlFlows.Exit;

namespace RoystonGame.TV.ControlFlows.Enter
{
    public class StateEntrance : StateOutlet
    {
        public StateEntrance(StateExit stateExit = null): base(stateExit)
        {
            // Empty
        }
        public void Inlet(User user, UserStateResult stateResult, UserFormSubmission formSubmission)
        {
            this.Exit.Inlet(user, stateResult, formSubmission);
        }
    }
}
