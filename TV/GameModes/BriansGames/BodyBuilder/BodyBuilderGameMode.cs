using RoystonGame.TV.ControlFlows;
using RoystonGame.TV.DataModels;
using RoystonGame.TV.DataModels.Enums;
using RoystonGame.TV.DataModels.GameStates;
using RoystonGame.TV.Extensions;
using RoystonGame.TV.GameModes.BriansGames.BodyBuilder.DataModels;
using RoystonGame.TV.GameModes.BriansGames.BodyBuilder.GameStates;
using RoystonGame.Web.DataModels.Requests.LobbyManagement;
using RoystonGame.Web.DataModels.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.TV.GameModes.BriansGames.BodyBuilder
{
    public class BodyBuilderGameMode : IGameMode
    {
        private List<Setup_Person> PeopleList { get; set; } = new List<Setup_Person>();
        private GameState Setup { get; set; }
        private GameState Gameplay { get; set; }

        private GameState Scoreboard { get; set; }
        private Random rand { get; } = new Random();
        public BodyBuilderGameMode(Lobby lobby, List<ConfigureLobbyRequest.GameModeOptionRequest> gameModeOptions)
        {
            Setup = new Setup_GS(lobby: lobby, peopleList: this.PeopleList, gameModeOptions: gameModeOptions);
            Scoreboard = new ScoreBoardGameState(lobby: lobby, outlet: this.Outlet);
            Setup.AddStateEndingListener(() =>
            {
                Gameplay = new Gameplay_GS(lobby: lobby, setup_PeopleList: this.PeopleList, outlet: Scoreboard.Inlet);
                Setup.Transition(this.Gameplay);
            });
            this.EntranceState = Setup;
        }
    }
}
