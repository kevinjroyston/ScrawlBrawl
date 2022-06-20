using Backend.GameInfrastructure.DataModels;
using Backend.GameInfrastructure.DataModels.Users;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.APIs.DataModels.UnityObjects
{
    public class UnityUser
    {
        [JsonProperty("a")]
        public Guid Id => User.Id;

        [JsonProperty("b")]
        public string DisplayName => User.DisplayName;

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public DrawingObject DrawingObject => User.SelfPortrait;

        [JsonProperty("c")]
        public int Score => User.Score;

        [JsonProperty("d")]
        public int ScoreDeltaReveal => User.ScoreDeltaReveal;

        [JsonProperty("e")]
        public int ScoreDeltaScoreboard => User.ScoreDeltaScoreboard;

        [JsonProperty("f")]
        public UserActivity Activity => User.Activity;

        [JsonProperty("g")]
        public UserStatus Status => User.Status;
         
        private User User { get; set; }

        public UnityUser(User user)
        {
            this.User = user;
        }
    }
}
