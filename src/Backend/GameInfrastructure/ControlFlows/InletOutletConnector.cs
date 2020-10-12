using System;
using Backend.GameInfrastructure.DataModels;
using Backend.GameInfrastructure.DataModels.Enums;
using Backend.GameInfrastructure.DataModels.Users;
using Common.DataModels.Requests;


using Connector = System.Action<
    Backend.GameInfrastructure.DataModels.Users.User,
    Backend.GameInfrastructure.DataModels.Enums.UserStateResult,
    Common.DataModels.Requests.UserFormSubmission>;

namespace Backend.GameInfrastructure.ControlFlows
{
    /// <summary>
    /// Class whose sole purpose is to implement inlet and outlet interfaces in addition to executing a provided func.
    /// </summary>
    public class InletOutletConnector : StateOutlet, IInlet
    {
        private Connector InternalConnector { get; set; }

        public InletOutletConnector(Connector internalLogic)
        {
            InternalConnector = internalLogic;
        }

        public void Inlet(User user, UserStateResult stateResult, UserFormSubmission formSubmission)
        {
            this.InternalConnector(user, stateResult, formSubmission);
            this.Exit.Inlet(user, stateResult, formSubmission);
        }

        public void AddEntranceListener(Action listener)
        {
            throw new NotImplementedException();
        }

        public void AddPerUserEntranceListener(Action<User> listener)
        {
            throw new NotImplementedException();
        }
    }
}
