using Common.DataModels.Responses;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Common.DataModels.Requests.LobbyManagement
{
    public class CreateAndJoinLobbyRequest
    {
        [Required]
        [StringLength(maximumLength: 20, ErrorMessage = "Name too long")]
        public string DisplayName { get; set; }

        [Required]
        [StringLength(maximumLength: 10000000, ErrorMessage = "Drawing too big")]
        [RegularExpression(pattern: "^data:image/png;base64,[a-zA-Z0-9+/]+=*$", ErrorMessage = "Drawing invalid", MatchTimeoutInMilliseconds =50)]
        public string SelfPortrait { get; set; }
    }
}
