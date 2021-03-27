using Backend.GameInfrastructure.ControlFlows.Enter;
using Backend.GameInfrastructure.ControlFlows.Exit;
using Backend.GameInfrastructure.DataModels.Users;
using Backend.GameInfrastructure.Extensions;
using Common.Code.Extensions;
using Microsoft.AspNetCore.Server.IIS.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Backend.GameInfrastructure.DataModels.States.StateGroups
{
    public class StateChain : StateGroup
    {
        private ConcurrentDictionary<User, int> UserChainIndex { get; } = new ConcurrentDictionary<User, int>();
        private int TotalChainLength { get; set; } = 0;
        private int CurrentMaxChainIndex { get; set; } = 0;

        // State chain constructors do not support any form of empty lists or null.
        public StateChain(List<State> states, StateExit exit = null, TimeSpan? stateDuration = null) : base(firstState:states.FirstOrDefault(),lastState:states.LastOrDefault(),exit: exit, stateTimeoutDuration: stateDuration)
        {
            TotalChainLength = states.Count;
            for (int i = 1; i < states.Count; i++)
            {
                states[i - 1].Transition(states[i]);
            }

            for (int i = 0; i < states.Count; i++)
            {
                int lambdaSafeIndex = i;
                states[i].AddPerUserEntranceListener((User user) =>
                {
                    this.UserChainIndex.AddOrReplace(user, lambdaSafeIndex);
                });
                states[i].AddEntranceListener(() =>
                {
                    this.CurrentMaxChainIndex = lambdaSafeIndex;
                });
            }
        }

        public StateChain(Func<int, State> stateGenerator, StateExit exit = null, TimeSpan? stateDuration = null) : base(exit: exit, stateTimeoutDuration: stateDuration)
        {
            this.Entrance.Transition(ChainCounter(counter: 0, stateGenerator: stateGenerator));
            this.TotalChainLength = -1;
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
                    this.TotalChainLength = counter;
                    return this.Exit;
                }

                toReturn.AddPerUserEntranceListener((User user) =>
                {
                    this.UserChainIndex.AddOrReplace(user, counter);
                });
                toReturn.AddEntranceListener(() =>
                {
                    this.CurrentMaxChainIndex = counter;
                });

                toReturn.Transition(ChainCounter(counter + 1, stateGenerator));
                return toReturn;
            };
        }

        public override string GetSummary(User user)
        {
            return $"UserChainIndex:({this.UserChainIndex[user]}), TotalChainLength:({this.TotalChainLength}), CurrentMaxChainIndex:({this.CurrentMaxChainIndex})";
        }
    }
}
