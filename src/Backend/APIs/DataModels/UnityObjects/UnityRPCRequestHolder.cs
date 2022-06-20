using Common.DataModels.UnityObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.APIs.DataModels.UnityObjects
{
    public class UnityRPCRequestHolder
    {
        [JsonProperty("a")]
        public UnityView UnityView { get; set; }
        [JsonProperty("b")]
        public UnityImageList UnityImageList { get; set; }
        [JsonProperty("c")]
        public ConfigurationMetadata ConfigurationMetadata { get; set; }
        [JsonProperty("d")]
        public UnityUserStatuses UnityUserStatus { get; set; }
    }
}
