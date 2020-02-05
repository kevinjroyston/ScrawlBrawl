using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RoystonGame.Web.DataModels.Requests.LobbyManagement
{
    public class ConfigureLobbyRequest
    {
        public class GameModeOptionRequest
        {
            public string ShortAnswer { get; set; }
            public int? RadioAnswer { get; set; }
        }

        [Required]
        public List<GameModeOptionRequest> Options { get; set; }

        [Required]
        public int? GameMode { get; set; }

        [JsonExtensionData]
        public IDictionary<string, object> Unmapped { get; set; }
    }
}
