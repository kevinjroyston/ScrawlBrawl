using Backend.GameInfrastructure.DataModels.Users;
using Backend.GameInfrastructure.DataModels.Enums;
using Common.DataModels.Requests;
using Backend.GameInfrastructure.DataModels;
using Backend.GameInfrastructure.ControlFlows.Exit;

namespace Backend.GameInfrastructure.ControlFlows.Enter
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
