using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.DataModels.States.GameStates;
using RoystonGame.TV.DataModels.States.UserStates;
using RoystonGame.TV.Extensions;
using RoystonGame.Web.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
using RoystonGame.Web.DataModels.UnityObjects;
using RoystonGame.WordLists;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using static System.FormattableString;
using RoystonGame.TV.ControlFlows.Exit;
using RoystonGame.TV.DataModels;
using RoystonGame.TV.DataModels.States.StateGroups;
using RoystonGame.TV.GameModes.Common.DataModels;

namespace RoystonGame.TV.GameModes.KevinsGames.Mimic.GameStates
{
    public class DisplayOriginal_GS : GameState
    {
        public DisplayOriginal_GS(Lobby lobby, TimeSpan? displayTimeDuration, UserDrawing displayDrawing)
            : base(lobby,
                  stateTimeoutDuration: displayTimeDuration,
                  exit: new WaitForStateTimeoutDuration_StateExit())
        {
            this.UnityView = new UnityView(this.Lobby)
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.ShowDrawings },
                Title = new StaticAccessor<string> { Value = "Memorize this drawing" },
                UnityImages = new StaticAccessor<IReadOnlyList<UnityImage>>
                {
                    Value = new List<UnityImage>()
                    {
                        displayDrawing.GetUnityImage()
                    } 
                }
            };
            this.Entrance.Transition(this.Exit);
        }
    }
}
