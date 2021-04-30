using Common.DataModels.Enums;
using Common.DataModels.Requests.LobbyManagement;
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

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public GameModeId GameId { get; set; }
        public string Description { get; set; }
        public string Identifier { get; set; } // should be unique across games, used to lookup html info pages and store offline data related to game

        public GameModeAttributes Attributes { get; set; }

        public List<GameModeOptionResponse> Options { get; set; }

        /// <summary>
        /// Function that returns a list of html classes which should be hidden from the tutorial page based on current selected options.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public Func<List<string>> GetTutorialHiddenClasses { get; set; }


        /// <summary>
        /// This function will be called once prior to instantiation.
        /// Optimizations can be made here down the road if needed as the call can be memoized.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public Func<int, List<ConfigureLobbyRequest.GameModeOptionRequest>, IReadOnlyDictionary<GameDuration, TimeSpan>> GetGameDurationEstimates {get; set;}


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
