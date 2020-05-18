using System.ComponentModel.DataAnnotations;

namespace RoystonGame.Web.DataModels.Requests.Other
{
    public class FeedbackRequest
    {
        [Required]
        public string? Feedback { get; }
    }
}
