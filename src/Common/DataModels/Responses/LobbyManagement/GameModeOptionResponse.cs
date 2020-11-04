using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Common.DataModels.Responses
{
    public class GameModeOptionResponse
    {
        public string Description { get; set; }

        public ResponseType ResponseType { get; set; }

        public int? MinValue { get; set; }

        public int? MaxValue { get; set; }
        public bool Optional { get; set; } = false;

        [JsonProperty("value")]
        public object DefaultValue { get; set; }
    }
    public class GameModeAttributes
    {
        public bool ProductionReady { get; set; } = false;
        public bool SupportsAudience { get; set; } = false;
        public bool SupportsTimers { get; set; } = false;
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ResponseType
    {
        Boolean = 0,
        Integer,
        Text,
    }
}
