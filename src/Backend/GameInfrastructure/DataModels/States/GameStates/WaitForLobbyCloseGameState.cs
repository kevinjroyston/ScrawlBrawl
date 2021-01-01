using Backend.GameInfrastructure.ControlFlows.Enter;
using Backend.GameInfrastructure.ControlFlows.Exit;
using Backend.GameInfrastructure.DataModels.Users;
using Backend.GameInfrastructure.Extensions;
using Backend.APIs.DataModels.UnityObjects;
using System.Collections.Generic;
using System.Linq;
using static System.FormattableString;
using Common.DataModels.Enums;
using Common.Code.Validation;

namespace Backend.GameInfrastructure.DataModels.States.GameStates
{
    public class WaitForLobbyCloseGameState : GameState
    {
        public void LobbyHasClosed()
        {
            ((WaitForTrigger_StateExit)this.Exit)?.Trigger();
        }

        public WaitForLobbyCloseGameState(Lobby lobby, StateEntrance entrance = null)
            : base(
                  lobby: lobby,
                  entrance: entrance,
                  exit: new WaitForTrigger_StateExit())
        {
            Arg.NotNull(lobby, nameof(lobby));

            this.Entrance.Transition(this.Exit);

            // I have created a monstrosity.
            this.Legacy_UnityView = new Legacy_UnityView(this.Lobby)
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.WaitForLobbyToStart },
                Title = new StaticAccessor<string> { Value = Invariant($"Lobby code: {lobby.LobbyId}") },
                Instructions = new StaticAccessor<string> { Value = "Players joined so far:" },
                UnityImages = new DynamicAccessor<IReadOnlyList<Legacy_UnityImage>>
                {
                    DynamicBacker = () => this.Lobby.GetAllUsers().OrderBy((User user)=>user.LobbyJoinTime).Select(usr =>
                        new Legacy_UnityImage(usr.Id)
                        {
                            Title = new StaticAccessor<string> { Value = usr.DisplayName },
                            Base64Pngs = new StaticAccessor<IReadOnlyList<string>>
                            {
                                Value = new List<string> { usr.SelfPortrait }
                            }
                        }).ToList()
                }
            };
        }
    }
}
