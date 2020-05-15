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
    /// Class whose sole purpose is to implement inlet and outlet interfaces in addition to executing a provided func.
    /// </summary>
    public class InletOutletConnector : StateOutlet, Inlet
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
    }
}
