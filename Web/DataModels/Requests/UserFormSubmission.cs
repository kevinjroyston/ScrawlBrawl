using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RoystonGame.Web.DataModels.Requests
{
    public class UserFormSubmission
    {
        [RegexSanitizer("^([0-9A-Fa-f]{8}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{12})$")]
        public Guid Id { get; set; }

        public UserSubForm[] SubForms { get; set; }

        [JsonExtensionData]
        public IDictionary<string, object> Unmapped { get; set; }
    }
}
