using System.Collections.Generic;
using static Common.DataModels.Requests.LobbyManagement.ConfigureLobbyRequest;

namespace BackendAutomatedTestingClient.DataModels
{
    public class TestOptions
    {
        public int NumPlayers { get; set; }
        public IReadOnlyList<GameModeOptionRequest> GameModeOptions { get; set; }
    }
}
