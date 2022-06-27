using Newtonsoft.Json;

namespace Backend.APIs.DataModels.UnityObjects
{
    public class UnityRoundDetails
    {
        [JsonProperty("a")]
        public int CurrentRound { get; set; }
        [JsonProperty("b")]
        public int TotalRounds { get; set; }
    }
}
