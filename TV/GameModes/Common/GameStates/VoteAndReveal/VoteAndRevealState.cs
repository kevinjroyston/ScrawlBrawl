using RoystonGame.TV.DataModels.States.StateGroups;
using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.Extensions;
using RoystonGame.TV.GameModes.Common.DataModels.Voting;
using RoystonGame.Web.DataModels.UnityObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.TV.GameModes.Common.GameStates.VoteAndReveal
{
    public class VoteAndRevealState<T> : StateGroup
    {
        
        public VoteAndRevealState(Lobby lobby, VoteableObjectHolder<T> voteableObjectHolder, List<User> votingUsers = null, TimeSpan? votingTime = null)
        {
            this.Entrance.AddExitListener(() =>
            {
                voteableObjectHolder.SetStartTime();
            });

            StateChain VoteAndRevealChainGenerator()
            {
                StateChain voteAndRevealStateChain = new StateChain(stateGenerator: (int counter) =>
                {
                    if (counter == 0)
                    {
                        return new VotingGameState(
                            lobby: lobby,
                            votingUsers: votingUsers,
                            votingUserPromptGenerator: voteableObjectHolder.VotingPromptGenerator,
                            votingFormSubmitHandler: voteableObjectHolder.VotingFormSubmitHandler,
                            votingTimeoutHandler: voteableObjectHolder.VotingTimeoutHandler,
                            votingExitListener: voteableObjectHolder.VotingExitListener,
                            votingUnityView: voteableObjectHolder.VotingUnityViewGenerator(),
                            waitingPromptGenerator: voteableObjectHolder.VotingWaitingPromptGenerator,
                            votingTime: votingTime);
                    }
                    else if (counter == 1)
                    {
                        return new VoteRevealGameState(
                            lobby: lobby,
                            voteRevealUnityView: voteableObjectHolder.RevealUnityViewGenerator(),
                            waitingPromptGenerator: voteableObjectHolder.RevealWaitingPromptGenerator);
                    }
                    else
                    {
                        return null;
                    }
                });
                voteAndRevealStateChain.Transition(this.Exit);
                return voteAndRevealStateChain;
            }

            this.Entrance.Transition(VoteAndRevealChainGenerator);
        }
    }
}
