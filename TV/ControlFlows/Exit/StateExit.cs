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
namespace RoystonGame.TV.ControlFlows.Exit
{
    public class StateExit : Inlet
    {
        private Connector InternalOutlet { get; set; }
        private List<Action> Listeners { get; set; } = new List<Action>();
        private bool CalledListeners { get; set; } = false;

        public void SetInternalOutlet(Connector internalOutlet)
        {
            this.InternalOutlet = internalOutlet;
        }

        public void Inlet(User user, UserStateResult stateResult, UserFormSubmission formSubmission)
        {
            if (!CalledListeners)
            {
                foreach (Action listener in Listeners)
                {
                    listener.Invoke();
                }
            }
            this.InternalOutlet(user, stateResult, formSubmission);
        }

        public void AddListener(Action listener)
        {
            if (CalledListeners)
            {
                throw new Exception("Too late to add a listener!");
            }
            Listeners.Add(listener);
        }
    }
}
