using Assets.Scripts.Networking.DataModels.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;

namespace Assets.Scripts.Networking.DataModels
{
    public enum UserActivity
    {
        Active,
        Inactive
    }
    public enum UserStatus
    {
        AnsweringPrompts,
        Waiting,
    }

    public class UnityUser
    {
        
        [JsonProperty("a")]
        public Guid Id { get; set; }
        
        [JsonProperty("b")]
        public string DisplayName { get; set; }
/*        public string SelfPortrait { get; set; } */

        public Sprite SelfPortraitSprite
        {
            get
            {
                return InternalSelfPortraitSprite ?? (InternalSelfPortraitSprite = TextureUtilities.LoadTextureFromBase64(Id.ToString()));
            }
        }
        private Sprite InternalSelfPortraitSprite = null;
        
        [JsonProperty("c")]
        public int Score { get; set; }

        [JsonProperty("d")]
        public int ScoreDeltaReveal { get; set; }

        [JsonProperty("e")]
        public int ScoreDeltaScoreboard { get; set; }

        [JsonProperty("f")]
        public UserActivity Activity {get; set;}

        [JsonProperty("g")]
        public UserStatus Status { get; set; }

    }
}
