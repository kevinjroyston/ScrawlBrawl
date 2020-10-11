using Backend.GameInfrastructure.ControlFlows.Enter;
using Backend.GameInfrastructure.ControlFlows.Exit;
using Backend.GameInfrastructure.Extensions;
using System;
using System.Collections.Generic;

namespace Backend.GameInfrastructure.DataModels.States.StateGroups
{
    public class StateGroup : State
    {
        // TODO: Add timeout logic, 
        protected StateGroup(TimeSpan? stateTimeoutDuration = null, StateEntrance entrance = null, StateExit exit = null) : base(stateTimeoutDuration: stateTimeoutDuration, entrance: entrance, exit: exit)
        {
            // Child classes using this are expected to set transitions up themselves.
        }

        public StateGroup(IInlet firstState, IOutlet lastState, TimeSpan? stateTimeoutDuration = null, StateEntrance entrance = null, StateExit exit = null) : base(stateTimeoutDuration: stateTimeoutDuration, entrance: entrance, exit: exit)
        {
            this.Entrance.Transition(firstState);
            lastState.Transition(this.Exit);
        }
        public StateGroup(Func<IInlet> firstState, IOutlet lastState, TimeSpan? stateTimeoutDuration = null, StateEntrance entrance = null, StateExit exit = null) : base(stateTimeoutDuration: stateTimeoutDuration, entrance: entrance, exit: exit)
        {
            this.Entrance.Transition(firstState);
            lastState.Transition(this.Exit);
        }
        public StateGroup(IInlet firstState, List<IOutlet> lastStates, TimeSpan? stateTimeoutDuration = null, StateEntrance entrance = null, StateExit exit = null) : base(stateTimeoutDuration: stateTimeoutDuration, entrance: entrance, exit: exit)
        {
            this.Entrance.Transition(firstState);
            foreach (IOutlet lastState in lastStates)
            {
                lastState.Transition(this.Exit);
            }
        }
        public StateGroup(Func<IInlet> firstState, List<IOutlet> lastStates, TimeSpan? stateTimeoutDuration = null, StateEntrance entrance = null, StateExit exit = null) : base(stateTimeoutDuration: stateTimeoutDuration, entrance: entrance, exit: exit)
        {
            this.Entrance.Transition(firstState);
            foreach (IOutlet lastState in lastStates)
            {
                lastState.Transition(this.Exit);
            }
        }
    }
}
