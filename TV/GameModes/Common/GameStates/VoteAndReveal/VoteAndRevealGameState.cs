using RoystonGame.TV.ControlFlows.Exit;
using RoystonGame.TV.DataModels.States.GameStates;
using RoystonGame.TV.DataModels.States.StateGroups;
using RoystonGame.TV.DataModels.States.UserStates;
using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.Extensions;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
using RoystonGame.Web.DataModels.UnityObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.TV.GameModes.Common.GameStates.VoteAndReveal
{
    public abstract class VoteAndRevealGameState : StateGroup
    {
        //public VoteAndRevealGameState(Lobby lobby, List<T> , List<User> votingUsers,  )
        protected DateTime startingTime;
        public VoteAndRevealGameState(Lobby lobby, TimeSpan? votingTime = null)
        {
            this.Entrance.AddExitListener(() =>
            {
                startingTime = DateTime.UtcNow;
            });

            StateChain voteAndRevealChain = new StateChain(
                stateGenerator: (int counter) =>
                {
                    if (counter == 0)
                    {
                        return new VotingGameState(
                            lobby: lobby,
                            votingUserPromptGenerator: VotingUserPromptGenerator,
                            votingFormSubmitHandler: VotingFormSubmitHandler,
                            votingTimeoutHandler: VotingTimeoutHandler,
                            votingExitListener: VotingExitListener,
                            votingUnityView: VotingUnityViewGenerator(),
                            waitingPromptGenerator: VotingWaitingPromptGenerator,
                            votingTime: votingTime);
                    }
                    else if (counter == 1)
                    {
                        return new VoteRevealGameState(
                            lobby: lobby,
                            voteRevealUnityView: VoteRevealUnityViewGenerator(),
                            waitingPromptGenerator: VoteRevealWaitingPromptGenerator);
                    }
                    else
                    {
                        return null;
                    }
                });
        }
        
        public abstract UserPrompt VotingUserPromptGenerator(User user);
        public abstract (bool, string) VotingFormSubmitHandler(User user, UserFormSubmission submission);
        public abstract void VotingTimeoutHandler(User user, UserFormSubmission submission);
        public abstract void VotingExitListener();
        public abstract UnityView VotingUnityViewGenerator();
        public virtual Func<User, UserPrompt> VotingWaitingPromptGenerator { get; set; } = null;
        public abstract UnityView VoteRevealUnityViewGenerator();
        public virtual Func<User, UserPrompt> VoteRevealWaitingPromptGenerator { get; set; } = null;


    }
}
