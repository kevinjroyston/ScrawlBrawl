using RoystonGame.TV.ControlFlows.Enter;
using RoystonGame.TV.ControlFlows.Exit;
using RoystonGame.TV.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RoystonGame.TV.DataModels.States.StateGroups
{
    public class StateChain : StateGroup
    {
        public StateChain(List<State> states, StateEntrance entrance = null, StateExit exit = null) : base(firstState:states.First(),lastState:states.Last(),entrance: entrance, exit: exit)
        {
            for (int i = 1; i < states.Count; i++)
            {
                states[i - 1].Transition(states[i]);
            }
        }

        public StateChain(Func<int, State> stateGenerator, StateEntrance entrance = null, StateExit exit = null) : base(entrance: entrance, exit: exit)
        {
            this.Entrance.Transition(ChainCounter(counter: 0, stateGenerator: stateGenerator));
        }

        private Func<IInlet> ChainCounter(int counter, Func<int, State> stateGenerator)
        {
            return () =>
            {
                if (counter > 50)
                {
                    throw new Exception("Max StateChain length (50) hit");
                }
                State toReturn = stateGenerator(counter);
                if (toReturn == null)
                {
                    return this.Exit;
                }
                toReturn.Transition(ChainCounter(counter + 1, stateGenerator));
                return toReturn;
            };
        }
    }
}
