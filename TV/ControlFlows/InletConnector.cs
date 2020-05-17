using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Connector = System.Action<
    RoystonGame.TV.DataModels.Users.User,
    RoystonGame.TV.DataModels.Enums.UserStateResult,
    RoystonGame.Web.DataModels.Requests.UserFormSubmission>;
using RoystonGame.TV.DataModels;

namespace RoystonGame.TV.ControlFlows
{
    /// <summary>
    /// Class whose sole purpose is to implement inlet interface to just use a standard connector.
    /// </summary>
    public class InletConnector : Inlet
    {
        private Connector InternalConnector { get; set; }

        public InletConnector(Connector internalConnector = null)
        {
            InternalConnector = internalConnector;
        }

        public void Inlet(User user, UserStateResult stateResult, UserFormSubmission formSubmission)
        {
            if (this.InternalConnector != null)
            {
                this.InternalConnector(user, stateResult, formSubmission);
            }
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
