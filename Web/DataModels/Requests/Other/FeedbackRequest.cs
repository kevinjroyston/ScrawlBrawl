using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RoystonGame.Web.DataModels.Requests.Other
{
    public class FeedbackRequest
    {
        [Required]
        public string? Feedback { get; }
    }
}
