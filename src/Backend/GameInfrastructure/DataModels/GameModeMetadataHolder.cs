using System;
using System.Collections.Generic;

using Common.DataModels.Requests.LobbyManagement;
using Backend.GameInfrastructure.DataModels.States.StateGroups;
using Common.DataModels.Responses;

namespace Backend.GameInfrastructure.DataModels
{
    public class GameModeMetadataHolder
    {
        public GameModeMetadata GameModeMetadata { get; set; }
        public Func<Lobby, List<ConfigureLobbyRequest.GameModeOptionRequest>, IGameMode> GameModeInstantiator { get; set; }
    }
}
