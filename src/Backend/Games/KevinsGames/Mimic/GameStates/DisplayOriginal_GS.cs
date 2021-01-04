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
            this.Legacy_UnityView = new Legacy_UnityView(this.Lobby)
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.ShowDrawings },
                Title = new StaticAccessor<string> { Value = "Memorize this drawing" },
                UnityImages = new StaticAccessor<IReadOnlyList<Legacy_UnityImage>>
                {
                    Value = new List<Legacy_UnityImage>()
                    {
                        displayDrawing.GetUnityImage()
                    } 
                }
            };
            this.Entrance.Transition(this.Exit);
        }
    }
}
