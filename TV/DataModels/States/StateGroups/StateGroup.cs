using RoystonGame.TV.ControlFlows.Enter;
using RoystonGame.TV.ControlFlows.Exit;
using RoystonGame.TV.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.TV.DataModels.States.StateGroups
{
    public class StateGroup : State
    {
        // TODO: Add timeout logic, 
        protected StateGroup(StateEntrance entrance = null, StateExit exit = null) : base(entrance: entrance, exit: exit)
        {
            // Child classes using this are expected to set transitions up themselves.
        }

        public StateGroup(Inlet firstState, Outlet lastState, StateEntrance entrance = null, StateExit exit = null) : base(entrance: entrance, exit: exit)
        {
            this.Entrance.Transition(firstState);
            lastState.Transition(this.Exit);
        }
        public StateGroup(Func<Inlet> firstState, Outlet lastState, StateEntrance entrance = null, StateExit exit = null) : base(entrance: entrance, exit: exit)
        {
            this.Entrance.Transition(firstState);
            lastState.Transition(this.Exit);
        }
        public StateGroup(Inlet firstState, List<Outlet> lastStates, StateEntrance entrance = null, StateExit exit = null) : base(entrance: entrance, exit: exit)
        {
            this.Entrance.Transition(firstState);
            foreach (Outlet lastState in lastStates)
            {
                lastState.Transition(this.Exit);
            }
        }
        public StateGroup(Func<Inlet> firstState, List<Outlet> lastStates, StateEntrance entrance = null, StateExit exit = null) : base(entrance: entrance, exit: exit)
        {
            this.Entrance.Transition(firstState);
            foreach (Outlet lastState in lastStates)
            {
                lastState.Transition(this.Exit);
            }
        }
    }
}
