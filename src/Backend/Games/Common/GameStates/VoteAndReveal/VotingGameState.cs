using Backend.GameInfrastructure.ControlFlows.Exit;
using Backend.GameInfrastructure.DataModels.States.GameStates;
using Backend.GameInfrastructure.DataModels.States.UserStates;
using Backend.GameInfrastructure.DataModels.Users;
using Backend.GameInfrastructure.Extensions;
using Common.DataModels.Requests;
using Common.DataModels.Responses;
using Backend.APIs.DataModels.UnityObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using Backend.GameInfrastructure;

namespace Backend.Games.Common.GameStates.VoteAndReveal
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
            List<User> votingUsers,
            Func<User, UserPrompt> waitingPromptGenerator = null,
            TimeSpan? votingTime = null) : base(lobby)
        {
            SelectivePromptUserState votingUserState = new SelectivePromptUserState(
                usersToPrompt: votingUsers ?? lobby.GetAllUsers().ToList(),
                promptGenerator: votingUserPromptGenerator,
                formSubmitHandler: votingFormSubmitHandler,
                userTimeoutHandler: (user, userFormSubmission) => {
                    votingTimeoutHandler(user, userFormSubmission);
                    return GameInfrastructure.DataModels.Enums.UserTimeoutAction.None;
                },
                maxPromptDuration: votingTime,
                exit: new WaitForUsers_StateExit(lobby: lobby, waitingPromptGenerator: waitingPromptGenerator));

            this.Entrance.Transition(votingUserState);
            votingUserState.Transition(this.Exit);


            votingUserState.AddExitListener(votingExitListener);

            this.UnityView = votingUnityView;
        }
    }
}
