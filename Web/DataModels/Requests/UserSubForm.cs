using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace RoystonGame.Web.DataModels.Requests
{
    public class UserSubForm
    {
        public Guid Id { get; set; }

        public string ShortAnswer { get; set; }

        public string Drawing { get; set; }

        public int? RadioAnswer { get; set; }

        public string Color { get; set; }

        [JsonExtensionData]
        public IDictionary<string, object> Unmapped { get; set; }
    }
}
