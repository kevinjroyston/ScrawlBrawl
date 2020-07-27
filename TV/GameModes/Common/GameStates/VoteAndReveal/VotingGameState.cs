using RoystonGame.TV.ControlFlows.Exit;
using RoystonGame.TV.DataModels.States.GameStates;
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
    public class VotingGameState: GameState
    {
        public VotingGameState(
            Lobby lobby,
            Func<User, UserPrompt> votingUserPromptGenerator,
            Func<User, UserFormSubmission, (bool, string)> votingFormSubmitHandler,
            Action<User, UserFormSubmission> votingTimeoutHandler,
            Action votingExitListener,
            UnityView votingUnityView,
            Func<User, UserPrompt> waitingPromptGenerator = null,
            TimeSpan? votingTime = null) : base(lobby)
        {
            SimplePromptUserState votingUserState = new SimplePromptUserState(
                promptGenerator: votingUserPromptGenerator,
                formSubmitHandler: votingFormSubmitHandler,
                //userTimeoutHandler: votingTimeoutHandler,
                maxPromptDuration: votingTime,
                exit: new WaitForUsers_StateExit(lobby: lobby, waitingPromptGenerator: waitingPromptGenerator));

            this.Entrance.Transition(votingUserState);
            votingUserState.Transition(this.Exit);


            votingUserState.AddExitListener(votingExitListener);

            this.UnityView = votingUnityView;
        }
    }
}
