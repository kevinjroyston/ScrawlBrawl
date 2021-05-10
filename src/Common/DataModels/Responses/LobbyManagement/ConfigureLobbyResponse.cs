using System;
using System.Collections.Generic;
using System.Text;
using Common.DataModels.Enums;
namespace Common.DataModels.Responses.LobbyManagement
{
    public class ConfigureLobbyResponse
    {
        public string LobbyId { get; set; }

        public int PlayerCount { get; set; }
        public IReadOnlyDictionary<GameDuration, int> GameDurationEstimatesInMinutes { get; set; }
    }
}
