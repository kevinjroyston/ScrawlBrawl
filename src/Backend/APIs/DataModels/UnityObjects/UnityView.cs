using Backend.APIs.DataModels.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel;
using Backend.GameInfrastructure;
using Common.DataModels.Enums;

namespace Backend.APIs.DataModels.UnityObjects
{
    public class UnityView : OptionsInterface<UnityViewOptions>
    {
        // TODO: standardize on one json framework.
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        private Lobby Lobby { get; }

        public UnityField<IReadOnlyList<UnityObject>> UnityObjects { get; set; } = null;
        public TVScreenId? ScreenId { get; set; }
        public Guid Id { get; } = Guid.NewGuid();
        public IReadOnlyList<UnityUser> Users { get; set; } = new List<UnityUser>().AsReadOnly();
        public UnityField<string> Title { get; set; }
        
        public UnityField<string> Instructions { get; set; }
        
        public DateTime? ServerTime { get { return DateTime.UtcNow; } }
        
        private DateTime? OverrideStateEndTime { get; set; }

        public DateTime? StateEndTime
        {
            get => OverrideStateEndTime ?? this.Lobby?.GetAllUsers().Select(user => user.EarliestStateTimeout).Append(this.Lobby.GetCurrentGameState().ApproximateStateEndTime).Min();
            set => OverrideStateEndTime = value;
        }
        
        public bool IsRevealing { get; set; } = false;
        
        public Dictionary<UnityViewOptions, object> Options { get; set; } = new Dictionary<UnityViewOptions, object>();

        public UnityView(Lobby lobby)
        {
            this.Lobby = lobby;
            if (lobby != null)
            {
                this.Users = Lobby.GetAllUsers().Select(user => new UnityUser(user)).ToList().AsReadOnly();
            }
        }
    }
}
