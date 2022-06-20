using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.APIs.DataModels.UnityObjects
{
    public class UnityImageList
    {
        [JsonProperty("a")]
        public Dictionary<string, string> ImgList { get; set; } = new Dictionary<string, string>();
    }
}
