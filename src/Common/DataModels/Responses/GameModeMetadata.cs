using Common.DataModels.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

using static System.FormattableString;

namespace Common.DataModels.Responses
{
    public class GameModeMetadata
    {
        public int? MinPlayers { get; set; }
        public int? MaxPlayers { get; set; }
        public string Title { get; set; }
        public string GameIdString { get { return this.GameId.ToString(); } }

        [JsonIgnore]
        public GameModeId GameId { get; set; }
        public string Description { get; set; }
        public string Identifier { get; set; } // should be unique across games, used to lookup html info pages and store offline data related to game

        public List<GameModeOptionResponse> Options { get; set; }

        /// <summary>
        /// An enum expected to be 0-indexed which will be used for accessing GameMode options.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public Type OptionsEnum { get; set; }
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
                restrictions.Add(Invariant($"Min Players: ({MinPlayers.Value})"));
            }

            if (MaxPlayers.HasValue)
            {
                restrictions.Add(Invariant($"Max Players: ({MaxPlayers.Value})"));
            }

            if (restrictions.Count == 0)
            {
                return "No player count restrictions";
            }

            return Invariant($"{string.Join("| ", restrictions)}");
        }
    }
}
