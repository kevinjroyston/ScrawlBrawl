using RoystonGame.TV;
using System;
using System.Collections.Generic;
using System.Linq;
using static RoystonGame.Web.DataModels.Requests.LobbyManagement.ConfigureLobbyRequest;

namespace RoystonGame.Web.DataModels.Responses
{
    public class LobbyMetadataResponse
    {
        /// <summary>
        /// Guid to uniquely identify a prompt/formSubmit pair.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        public string LobbyId { get; }

        public int PlayerCount { get; }

        public List<GameModeMetadata> GameModes { get; } = Lobby.GameModes.ToList();

        public int? SelectedGameMode { get; }
        public List<GameModeOptionRequest> CurrentGameModeOptions { get; }

        public LobbyMetadataResponse(Lobby lobby)
        {
            this.LobbyId = lobby.LobbyId;
            this.PlayerCount = lobby.GetAllUsers().Count();
            this.SelectedGameMode = lobby.SelectedGameMode;
            this.CurrentGameModeOptions = lobby.GameModeOptions;
        }
    }
}
