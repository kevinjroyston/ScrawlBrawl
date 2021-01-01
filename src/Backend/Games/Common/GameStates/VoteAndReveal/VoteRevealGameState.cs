using Backend.GameInfrastructure.ControlFlows.Exit;
using Backend.GameInfrastructure.DataModels.States.GameStates;
using Backend.GameInfrastructure.DataModels.Users;
using Backend.GameInfrastructure.Extensions;
using Common.DataModels.Responses;
using Backend.APIs.DataModels.UnityObjects;
using System;
using Backend.GameInfrastructure;
using Common.DataModels.Enums;

namespace Backend.Games.Common.GameStates.VoteAndReveal
{
    public class VoteRevealGameState : GameState
    {
        private static UserPrompt PartyLeaderSkipButton(User user) => new UserPrompt()
        {
            UserPromptId = UserPromptId.PartyLeader_SkipReveal,
            Title = "Skip Reveal",
            SubmitButton = true
        };

        public VoteRevealGameState(
            Lobby lobby,
            Legacy_UnityView voteRevealUnityView,
            Func<User, UserPrompt> waitingPromptGenerator = null) : base(
                lobby: lobby,
                exit: new WaitForPartyLeader_StateExit(
                    lobby: lobby,
                    partyLeaderPromptGenerator: PartyLeaderSkipButton,
                    waitingPromptGenerator: waitingPromptGenerator))
        {
            this.Entrance.Transition(this.Exit);
            this.UnityView = voteRevealUnityView;
        }
    }
}
