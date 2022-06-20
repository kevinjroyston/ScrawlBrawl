using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;


namespace Assets.Scripts.Networking.DataModels
{
    public class UnityImageList
    {
        [JsonProperty("a")]
        public Dictionary<string, string> ImgList { get; set; }
    }
}
