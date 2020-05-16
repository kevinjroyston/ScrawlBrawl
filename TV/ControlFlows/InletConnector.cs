using RoystonGame.TV.DataModels;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Connector = System.Action<
    RoystonGame.TV.DataModels.User,
    RoystonGame.TV.DataModels.Enums.UserStateResult,
    RoystonGame.Web.DataModels.Requests.UserFormSubmission>;

namespace RoystonGame.TV.ControlFlows
{
    /// <summary>
    /// Class whose sole purpose is to implement inlet interface to just use a standard connector.
    /// </summary>
    public class InletConnector : Inlet
    {
        private Connector InternalConnector { get; set; }

        public InletConnector(Connector internalConnector)
        {
            InternalConnector = internalConnector;
        }

        public void Inlet(User user, UserStateResult stateResult, UserFormSubmission formSubmission)
        {
            this.InternalConnector(user, stateResult, formSubmission);
        }
        public void AddListener(Action listener)
        {
            throw new NotImplementedException();
        }
    }
}
