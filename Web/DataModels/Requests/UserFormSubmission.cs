using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RoystonGame.Web.DataModels.Requests
{
    public class UserFormSubmission
    {
        public Guid Id { get; set; }

        public UserSubForm[] SubForms { get; set; }

        [JsonExtensionData]
        public IDictionary<string, object> Unmapped { get; set; }
    }
}
