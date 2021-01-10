using Backend.APIs.DataModels.UnityObjects;
using Backend.GameInfrastructure;
using Backend.GameInfrastructure.ControlFlows.Exit;
using Backend.GameInfrastructure.DataModels.States.GameStates;
using Backend.GameInfrastructure.DataModels.States.UserStates;
using Backend.GameInfrastructure.DataModels.Users;
using Backend.GameInfrastructure.Extensions;
using Common.DataModels.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Games.Common.GameStates.QueryAndReveal
{
    public class RevealGameState : GameState
    {
        public RevealGameState(
            Lobby lobby,
            string promptTitle,
            UnityView revealUnityView) : base(
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
            this.UnityView = revealUnityView;
        }
    }
}
