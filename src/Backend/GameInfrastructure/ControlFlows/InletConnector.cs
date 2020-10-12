using Backend.GameInfrastructure.DataModels.Users;
using Backend.GameInfrastructure.DataModels.Enums;
using Common.DataModels.Requests;
using System;

using Connector = System.Action<
    Backend.GameInfrastructure.DataModels.Users.User,
    Backend.GameInfrastructure.DataModels.Enums.UserStateResult,
    Common.DataModels.Requests.UserFormSubmission>;
using Backend.GameInfrastructure.DataModels;

namespace Backend.GameInfrastructure.ControlFlows
{
    /// <summary>
    /// Class whose sole purpose is to implement inlet interface to just use a standard connector.
    /// </summary>
    public class InletConnector : IInlet
    {
        private Connector InternalConnector { get; set; }

        public InletConnector(Connector internalConnector = null)
        {
            InternalConnector = internalConnector;
        }

        public void Inlet(User user, UserStateResult stateResult, UserFormSubmission formSubmission)
        {
            this.InternalConnector?.Invoke(user, stateResult, formSubmission);
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
