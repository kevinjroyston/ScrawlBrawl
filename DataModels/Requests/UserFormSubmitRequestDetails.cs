using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RoystonGame.DataModels.Requests
{
    public class UserFormSubmitRequestDetails
    {
        public Guid Id { get; set; }

        public UserSubForm[] SubForms { get; set; }

        [JsonExtensionData]
        public IDictionary<string, object> Unmapped { get; set; }
    }
}
