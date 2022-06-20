using Assets.Scripts.Networking.DataModels.Enums;
using Assets.Scripts.Networking.DataModels.UnityObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Assets.Scripts.Networking.DataModels
{
    public class UnityView : OptionsInterface<UnityViewOptions>
    {
        [JsonProperty("a")]
        public UnityField<List<object>> UnityObjects { get; set; }

        [JsonProperty("b")]
        public TVScreenId ScreenId { get; set; }

        [JsonProperty("c")]
        public Guid Id { get; set; }

        [JsonProperty("d")]
        public List<UnityUser> Users { get; set; }

        [JsonProperty("e")]
        public UnityField<string> Title { get; set; }

        [JsonProperty("f")]
        public UnityField<string> Instructions { get; set; }

        [JsonProperty("g")]
        public DateTime? ServerTime { get; set; }

        [JsonProperty("h")]
        public DateTime? StateEndTime { get; set; }

        [JsonProperty("i")]
        public bool IsRevealing { get; set; }

        [JsonProperty("j")]
        public Dictionary<UnityViewOptions, object> Options { get; set; }
    }
}
