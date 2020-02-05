using RoystonGame.TV;
using RoystonGame.TV.GameModes;
using RoystonGame.Web.DataModels.Requests.LobbyManagement;
using RoystonGame.Web.DataModels.Responses;
using System;
using System.Collections.Generic;

namespace RoystonGame.Web.DataModels
{
    public class GameModeMetadata
    {
        public int? MinPlayers { get; set; }
        public int? MaxPlayers { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public List<GameModeOptionResponse> Options { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public Func<Lobby, List<ConfigureLobbyRequest.GameModeOptionRequest>, IGameMode> GameModeInstantiator { get; set; }
    }
}
