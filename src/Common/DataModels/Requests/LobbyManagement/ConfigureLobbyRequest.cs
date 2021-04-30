using Common.DataModels.Responses;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Common.DataModels.Requests.LobbyManagement
{
    public class ConfigureLobbyRequest
    {
        public class GameModeOptionRequest
        {
            [Required]
            public string Value { get; set; } 

            [Newtonsoft.Json.JsonIgnore]
            [System.Text.Json.Serialization.JsonIgnore]
            public object ValueParsed { get; private set; }

            public bool ParseValue(GameModeOptionResponse response, out string errorMsg)
            {
                errorMsg = string.Empty;
                switch (response.ResponseType)
                {
                    case ResponseType.Boolean:
                        if (!bool.TryParse(Value, out bool parsedAsBool))
                        {
                            errorMsg = $"Invalid boolean received.";
                            return false;
                        }
                        ValueParsed = parsedAsBool;
                        break;

                    case ResponseType.Integer:
                        if (!int.TryParse(Value, out int parsedAsInt))
                        {
                            errorMsg = $"Invalid integer received.";
                            return false;
                        }

                        if (response.MinValue.HasValue && response.MinValue > parsedAsInt)
                        {
                            errorMsg = $"Value cannot be less than {response.MinValue}";
                            return false;
                        }
                        
                        if (response.MaxValue.HasValue && response.MaxValue < parsedAsInt)
                        {
                            errorMsg = $"Value cannot be greater than {response.MaxValue}";
                            return false;
                        }
                        ValueParsed = parsedAsInt;
                        break;

                    case ResponseType.Text:
                        if (string.IsNullOrWhiteSpace(Value))
                        {
                            errorMsg = "Invalid string received.";
                            return false;
                        }
                        ValueParsed = Value;
                        break;

                    default:
                        errorMsg = $"Invalid response type: {response.ResponseType}.";
                        return false;
                }
                return true;
            }
        }

        [Required]
        public List<GameModeOptionRequest> Options { get; set; }

        [Required]
        public StandardGameModeOptions StandardOptions { get; set; }

        [Required]
        public int? GameMode { get; set; }

        [JsonExtensionData]
        public IDictionary<string, object> Unmapped { get; set; }
    }
}
