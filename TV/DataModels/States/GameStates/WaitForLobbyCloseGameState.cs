using RoystonGame.TV.ControlFlows;
using RoystonGame.TV.ControlFlows.Enter;
using RoystonGame.TV.ControlFlows.Exit;
using RoystonGame.TV.Extensions;
using RoystonGame.Web.DataModels.Enums;
using RoystonGame.Web.DataModels.UnityObjects;
using RoystonGame.Web.Helpers.Validation;
using System.Collections.Generic;
using System.Linq;
using static System.FormattableString;
using Connector = System.Action<
    RoystonGame.TV.DataModels.Users.User,
    RoystonGame.TV.DataModels.Enums.UserStateResult,
    RoystonGame.Web.DataModels.Requests.UserFormSubmission>;

namespace RoystonGame.TV.DataModels.States.GameStates
{
    public class WaitForLobbyCloseGameState : GameState
    {
        public void LobbyHasClosed()
        {
            ((WaitForTrigger_StateExit)this.Exit)?.Trigger();
        }

        private WaitForTrigger_StateExit WaitingState { get; }

        public WaitForLobbyCloseGameState(Lobby lobby, StateEntrance entrance = null)
            : base(
                  lobby: lobby,
                  entrance: entrance,
                  exit: new WaitForTrigger_StateExit())
        {
            Arg.NotNull(lobby, nameof(lobby));

            this.Entrance.Transition(this.Exit);

            // I have created a monstrosity.
            this.UnityView = new UnityView
            {
                ScreenId = new StaticAccessor<TVScreenId> { Value = TVScreenId.WaitForLobbyToStart },
                Title = new StaticAccessor<string> { Value = Invariant($"Lobby code: {lobby.LobbyId}") },
                Instructions = new StaticAccessor<string> { Value = "Players joined so far:" },
                UnityImages = new DynamicAccessor<IReadOnlyList<UnityImage>>
                {
                    DynamicBacker = () => this.Lobby.GetAllUsers().Select(usr =>
                        new UnityImage
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
