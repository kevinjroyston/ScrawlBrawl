using RoystonGame.TV.ControlFlows.Enter;
using RoystonGame.TV.ControlFlows.Exit;
using RoystonGame.TV.Extensions;
using System;
using System.Collections.Generic;

namespace RoystonGame.TV.DataModels.States.StateGroups
{
    public class StateGroup : State
    {
        // TODO: Add timeout logic, 
        protected StateGroup(StateEntrance entrance = null, StateExit exit = null) : base(entrance: entrance, exit: exit)
        {
            // Child classes using this are expected to set transitions up themselves.
        }

        public StateGroup(IInlet firstState, IOutlet lastState, StateEntrance entrance = null, StateExit exit = null) : base(entrance: entrance, exit: exit)
        {
            this.Entrance.Transition(firstState);
            lastState.Transition(this.Exit);
        }
        public StateGroup(Func<IInlet> firstState, IOutlet lastState, StateEntrance entrance = null, StateExit exit = null) : base(entrance: entrance, exit: exit)
        {
            this.Entrance.Transition(firstState);
            lastState.Transition(this.Exit);
        }
        public StateGroup(IInlet firstState, List<IOutlet> lastStates, StateEntrance entrance = null, StateExit exit = null) : base(entrance: entrance, exit: exit)
        {
            this.Entrance.Transition(firstState);
            foreach (IOutlet lastState in lastStates)
            {
                lastState.Transition(this.Exit);
            }
        }
        public StateGroup(Func<IInlet> firstState, List<IOutlet> lastStates, StateEntrance entrance = null, StateExit exit = null) : base(entrance: entrance, exit: exit)
        {
            this.Entrance.Transition(firstState);
            foreach (IOutlet lastState in lastStates)
            {
                lastState.Transition(this.Exit);
            }
        }
    }
}
