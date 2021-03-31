using Common.DataModels.Responses;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Common.DataModels.Requests.LobbyManagement
{
    public class JoinLobbyRequest
    {

        [Required]
        public string LobbyID { get; set; }

        [Required]
        public string DisplayName { get; set; }

        [Required]
        public string SelfPortrait { get; set; }
    }
}
