using System;
using System.Collections.Generic;
using System.Text;
using static RoystonGame.Web.DataModels.Requests.LobbyManagement.ConfigureLobbyRequest;

namespace RoystonGameAutomatedTestingClient.DataModels
{
    public class TestOptions
    {
        public int NumPlayers { get; set; }
        public IReadOnlyList<GameModeOptionRequest> GameModeOptions { get; set; }
    }
}
