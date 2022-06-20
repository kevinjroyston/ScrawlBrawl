using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Assets.Scripts.Networking.DataModels
{
    public class UnityRPCDataHolder
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
