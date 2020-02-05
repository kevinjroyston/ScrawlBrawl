using System;
using System.Collections.Generic;
using System.Linq;
using RoystonGame.TV.ControlFlows;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.Web.DataModels.Enums;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.UnityObjects;
using static System.FormattableString;

using Connector = System.Action<
    RoystonGame.TV.DataModels.User,
    RoystonGame.TV.DataModels.Enums.UserStateResult,
    RoystonGame.Web.DataModels.Requests.UserFormSubmission>;

namespace RoystonGame.TV.DataModels.GameStates
{
    public class WaitForLobbyCloseGameState : GameState
    {
        public void LobbyHasClosed()
        {
            WaitingState?.Trigger();
        }

        private WaitForTrigger WaitingState { get; }

        public WaitForLobbyCloseGameState(Lobby lobby, Connector outlet = null) : base(lobby, outlet)
        {
            WaitForTrigger transition = new WaitForTrigger(outlet: this.Outlet);

            this.Entrance = transition;

            // I have created a monstrosity.
            this.UnityView = new UnityView
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.WaitForLobbyToStart },
                Title = new StaticAccessor<string> { Value = Invariant($"Lobby code: {lobby.LobbyId}") },
                Instructions = new StaticAccessor<string> { Value = "Players joined so far:" },
                UnityImages = new DynamicAccessor<IReadOnlyList<UnityImage>>
                {
                    DynamicBacker = () => this.Lobby.GetActiveUsers().Select(usr =>
                        new UnityImage
                        {
                            Title = new StaticAccessor<string> { Value = usr.DisplayName },
                            Base64Pngs = new StaticAccessor<IReadOnlyList<string>> {
                                Value = new List<string> { usr.SelfPortrait }
                            }
                        }).ToList()
                }
            };
        }
    }
}
