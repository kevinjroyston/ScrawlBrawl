using Newtonsoft.Json;

namespace Backend.APIs.DataModels.UnityObjects
{
    public class UnityRoundDetails
    {
        [JsonProperty("a")]
        public int TotalRounds { get; set; }
        [JsonProperty("b")]
        public int CurrentRound { get; set; }
    }
}
