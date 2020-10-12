using System.ComponentModel.DataAnnotations;

namespace Common.DataModels.Requests.Other
{
    public class FeedbackRequest
    {
        [Required]
        public string? Feedback { get; }
    }
}
