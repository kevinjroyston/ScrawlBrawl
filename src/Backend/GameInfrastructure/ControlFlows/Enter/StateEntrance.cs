using Backend.GameInfrastructure.DataModels.Users;
using Backend.GameInfrastructure.DataModels.Enums;
using Common.DataModels.Requests;
using Backend.GameInfrastructure.DataModels;
using Backend.GameInfrastructure.ControlFlows.Exit;
using System;

namespace Backend.GameInfrastructure.ControlFlows.Enter
{
    public class StateEntrance : StateOutlet
    {
        // Due to design constraints StateEntrance no longer supports providing a custom StateExit.
        // This feature was never used and will be tricky to support going forward.
        //
        public StateEntrance(): base(new StateExit())
        {
            // Empty.
        }
        public void Inlet(User user, UserStateResult stateResult, UserFormSubmission formSubmission)
        {
            this.Exit.Inlet(user, stateResult, formSubmission);
        }
    }
}
