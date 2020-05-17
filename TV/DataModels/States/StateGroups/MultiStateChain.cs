using RoystonGame.TV.ControlFlows.Enter;
using RoystonGame.TV.ControlFlows.Exit;
using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.Extensions;
using System;
using System.Collections.Generic;

namespace RoystonGame.TV.DataModels.States.StateGroups
{
    public class MultiStateChain : StateGroup
    {
        public MultiStateChain(Func<User, StateChain> stateChainGenerator, StateEntrance entrance = null, StateExit exit = null) : base(entrance: entrance, exit: exit)
        {
            this.Entrance.Transition((User user) =>
            {
                StateChain toReturn = stateChainGenerator(user);
                toReturn.Transition(this.Exit);
                return toReturn;
            });
        }
        public MultiStateChain(Func<User, List<State>> stateChainGenerator, StateEntrance entrance = null, StateExit exit = null) : base(entrance: entrance, exit: exit)
        {
            this.Entrance.Transition((User user) =>
            {
                StateChain toReturn = new StateChain(stateChainGenerator(user));
                toReturn.Transition(this.Exit);
                return toReturn;
            });
        }
    }
}
