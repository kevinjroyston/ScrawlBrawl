using Backend.APIs.DataModels.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel;
using Backend.GameInfrastructure;
using Common.DataModels.Enums;
using Backend.GameInfrastructure.DataModels.Users;
using Newtonsoft.Json;

namespace Backend.APIs.DataModels.UnityObjects
{
    public class UnityView : OptionsInterface<UnityViewOptions>
    {
        // TODO: standardize on one json framework.
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        private Lobby Lobby { get; }

        [JsonProperty("a")]
        public UnityField<IReadOnlyList<UnityObject>> UnityObjects { get; set; } = null;

        [JsonProperty("b")]
        public TVScreenId? ScreenId { get; set; }

        [JsonProperty("c")]
        public Guid Id { get; } = Guid.NewGuid();
        private IReadOnlyList<UnityUser> ConnectedUsers { get; set; } = new List<UnityUser>().AsReadOnly();


        [JsonProperty("d")]
        public IReadOnlyList<UnityUser> Users 
        {
            get { return ConnectedUsers.Where(user=>user.Activity != UserActivity.Disconnected).ToList(); }
            set { ConnectedUsers = value; }
        }

        [JsonProperty("e")]
        public UnityField<string> Title { get; set; }

        [JsonProperty("f")]
        public UnityField<string> Instructions { get; set; }

        [JsonProperty("g")]
        public DateTime? ServerTime { get { return DateTime.UtcNow; } }
        
        private DateTime? OverrideStateEndTime { get; set; }

        [JsonProperty("h")]
        public DateTime? StateEndTime
        {
            get => OverrideStateEndTime ?? this.Lobby?.GetAllUsers().Where(user => user.Activity != UserActivity.Disconnected && !user.Deleted).Select(user => user.EarliestStateTimeout).Append(this.Lobby.GetCurrentGameState().ApproximateStateEndTime).Min();
            set => OverrideStateEndTime = value;
        }

        [JsonProperty("i")]
        public bool IsRevealing { get; set; } = false;

        [JsonProperty("j")]
        public Dictionary<UnityViewOptions, object> Options { get; set; } = new Dictionary<UnityViewOptions, object>();

        [JsonProperty("k")]
        public UnityRoundDetails RoundDetails { get; set; }

        public UnityView(Lobby lobby)
        {
            this.Lobby = lobby;

            // For most states, this user list will not be changing. (Activity status of users may change but not the list)
            if (lobby != null)
            {
                this.Users = Lobby.GetAllUsers().Select(user => new UnityUser(user)).ToList().AsReadOnly();
            }
        }
    }
}
