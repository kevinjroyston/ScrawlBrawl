using RoystonGame.TV;
using RoystonGame.TV.DataModels.States.StateGroups;
using RoystonGame.Web.DataModels.Requests.LobbyManagement;
using RoystonGame.Web.DataModels.Responses;
using System;
using System.Collections.Generic;

using static System.FormattableString;

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

        public bool IsSupportedPlayerCount(int playerCount, bool ignoreMinimum = false)
        {
            if (!ignoreMinimum && (MinPlayers.HasValue && MinPlayers.Value > playerCount))
            {
                return false;
            }
            if (MaxPlayers.HasValue && MaxPlayers.Value < playerCount)
            {
                return false;
            }
            return true;
        }
        public string RestrictionsToString()
        {
            List<string> restrictions = new List<string>();

            if (MinPlayers.HasValue)
            {
                restrictions.Add(Invariant($"Min: ({MinPlayers.Value})"));
            }

            if (MaxPlayers.HasValue)
            {
                restrictions.Add(Invariant($"Max: ({MaxPlayers.Value})"));
            }

            if (restrictions.Count == 0)
            {
                return "No player count restrictions";
            }

            return Invariant($"{string.Join("| ", restrictions)}");
        }
    }
}
