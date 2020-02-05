using RoystonGame.TV;
using RoystonGame.TV.GameModes;
using RoystonGame.Web.DataModels.Requests.LobbyManagement;
using RoystonGame.Web.DataModels.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RoystonGame.Web.DataModels
{
    public class GameModeMetadata
    {
        public int? MinPlayers { get; set; }
        public int? MaxPlayers { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public List<GameModeOptionResponse> Options { get; set; }

        [JsonIgnore]
        public Func<Lobby, List<ConfigureLobbyRequest.GameModeOptionRequest>, IGameMode> GameModeInstantiator { get; set; }
    }
}
