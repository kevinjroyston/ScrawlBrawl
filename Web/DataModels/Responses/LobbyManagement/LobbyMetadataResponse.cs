using RoystonGame.TV;
using RoystonGame.Web.Helpers.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using static RoystonGame.Web.DataModels.Requests.LobbyManagement.ConfigureLobbyRequest;

namespace RoystonGame.Web.DataModels.Responses
{
    public class LobbyMetadataResponse
    {
        public string LobbyId { get; }

        public int PlayerCount { get; }
        public bool GameInProgress { get; }

        public GameModeMetadata GameModeSettings { get; }

        public int? SelectedGameMode { get; }

        public LobbyMetadataResponse(Lobby lobby)
        {
            this.LobbyId = lobby.LobbyId;
            this.PlayerCount = lobby.GetAllUsers().Count();
            this.GameInProgress = lobby.IsGameInProgress();
            // TODO: GameModeSettings isn't used
            this.GameModeSettings = lobby.SelectedGameMode;
            for (int i = 0; i < (this.GameModeSettings?.Options?.Count ?? 0); i++)
            {
                if (lobby?.GameModeOptions?[i]?.ValueParsed != null)
                {
                    this.GameModeSettings.Options[i].DefaultValue = lobby.GameModeOptions[i].Value;
                }
            }
            this.SelectedGameMode = Lobby.GameModes.FirstIndex((gameMode) => gameMode.Title.Equals(GameModeSettings?.Title, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
