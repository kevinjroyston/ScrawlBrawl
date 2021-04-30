using Common.DataModels.Enums;
using Common.DataModels.Responses;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Common.DataModels.Requests.LobbyManagement
{
    /// <summary>
    /// Class containing options common to all games.
    /// </summary>
    public class StandardGameModeOptions
    {
        [Required]
        public bool ShowTutorial { get; set; }

        /// <summary>
        /// Length of the game. (Games will decide how many of each prompt to show as well as timer duration).
        /// </summary>
        [Required]
        public GameDuration GameDuration { get; set; }

        [Required]
        public bool TimerEnabled { get; set; }
    }
}
