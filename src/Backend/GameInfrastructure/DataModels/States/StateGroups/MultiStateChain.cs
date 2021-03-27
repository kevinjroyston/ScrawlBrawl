using Backend.GameInfrastructure.ControlFlows.Enter;
using Backend.GameInfrastructure.ControlFlows.Exit;
using Backend.GameInfrastructure.DataModels.Users;
using Backend.GameInfrastructure.Extensions;
using System;
using System.Collections.Generic;

namespace Backend.GameInfrastructure.DataModels.States.StateGroups
{
    public class MultiStateChain : StateGroup
    {
        public MultiStateChain(Func<User, StateChain> stateChainGenerator, StateExit exit = null, TimeSpan? stateDuration = null) : base( exit: exit, stateTimeoutDuration: stateDuration)
        {
            this.Entrance.Transition((User user) =>
            {
                StateChain toReturn = stateChainGenerator(user);
                toReturn.Transition(this.Exit);
                return toReturn;
            });
        }
        public MultiStateChain(Func<User, List<State>> stateChainGenerator, StateExit exit = null, TimeSpan? stateDuration = null) : base(exit: exit, stateTimeoutDuration: stateDuration)
        {
            this.Entrance.Transition((User user) =>
            {
                List<State> statesList = stateChainGenerator(user);
                if(statesList.Count <= 0)
                {
                    // If no states in chain, go straight to exit.
                    return this.Exit;
                }

                StateChain toReturn = new StateChain(statesList);
                toReturn.Transition(this.Exit);
                return toReturn;
            });
        }
    }
}
