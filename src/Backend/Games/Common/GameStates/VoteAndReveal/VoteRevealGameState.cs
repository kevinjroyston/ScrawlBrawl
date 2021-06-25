using Backend.GameInfrastructure.ControlFlows.Exit;
using Backend.GameInfrastructure.DataModels.States.GameStates;
using Backend.GameInfrastructure.DataModels.Users;
using Backend.GameInfrastructure.Extensions;
using Common.DataModels.Responses;
using Backend.APIs.DataModels.UnityObjects;
using System;
using Backend.GameInfrastructure;
using Common.DataModels.Enums;
using System.Linq;
using Backend.GameInfrastructure.DataModels.States.UserStates;

namespace Backend.Games.Common.GameStates.VoteAndReveal
{
    public class VoteRevealGameState : GameState
    {
        public VoteRevealGameState(
            Lobby lobby,
            string promptTitle,
            UnityView voteRevealUnityView) : base(
                lobby: lobby,
                exit: new WaitForPartyLeader_StateExit(
                    lobby: lobby,
                    partyLeaderPromptGenerator: Prompts.ShowScoreBreakdowns(
                        lobby: lobby, 
                        promptTitle: promptTitle,
                        userPromptId: UserPromptId.PartyLeader_SkipReveal,
                        userScoreBreakdownScope: Score.Scope.Reveal,
                        showPartyLeaderSkipButton: true),

                    waitingPromptGenerator: Prompts.ShowScoreBreakdowns(
                        lobby: lobby,
                        promptTitle: promptTitle,
                        userScoreBreakdownScope: Score.Scope.Reveal)))
        {
            this.Entrance.Transition(this.Exit);
            this.UnityView = voteRevealUnityView;
        }
    }
}
