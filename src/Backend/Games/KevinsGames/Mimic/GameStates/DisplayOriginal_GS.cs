using Backend.GameInfrastructure.DataModels.States.GameStates;
using Backend.GameInfrastructure.Extensions;
using Backend.APIs.DataModels.UnityObjects;
using System;
using System.Collections.Generic;
using Backend.GameInfrastructure.ControlFlows.Exit;
using Backend.Games.Common.DataModels;
using Common.DataModels.Enums;
using Backend.GameInfrastructure;
using Backend.GameInfrastructure.DataModels.States.UserStates;

namespace Backend.Games.KevinsGames.Mimic.GameStates
{
    public class DisplayOriginal_GS : GameState
    {
        public DisplayOriginal_GS(Lobby lobby, TimeSpan? displayTimeDuration, UserDrawing displayDrawing)
            : base(lobby,
                  stateTimeoutDuration: displayTimeDuration,
                  exit: new WaitForStateTimeoutDuration_StateExit(waitingPromptGenerator:Prompts.DisplayText(Prompts.Text.LookAtTheScreen)))
        {
            this.UnityView = new UnityView(this.Lobby)
            {
                ScreenId = TVScreenId.ShowDrawings,
                Title = new UnityField<string> { Value = "Memorize this drawing" },
                UnityObjects = new UnityField<IReadOnlyList<UnityObject>>
                {
                    Value = new List<UnityObject>()
                    {
                        displayDrawing.GetUnityObject()
                    } 
                }
            };
            this.Entrance.Transition(this.Exit);
        }
    }
}
