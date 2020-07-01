using RoystonGame.TV;
using RoystonGame.TV.DataModels.Users;
using RoystonGame.Web.DataModels.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RoystonGame.Web.DataModels.UnityObjects
{
    public class UnityView
    {
        // TODO: standardize on one json framework.
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        private Lobby Lobby { get; }
        public UnityView(Lobby lobby)
        {
            this.Lobby = lobby;
            if(lobby != null)
            {
                this.Users = new DynamicAccessor<IReadOnlyList<User>> { DynamicBacker = () => Lobby.GetAllUsers() };
            }
        }
        public bool Refresh()
        {
            // First Refresh is always dirty.
            bool modified = this.Dirty;
            this.Dirty = false;

            modified |= this.Options?.Refresh() ?? false;
            modified |= this.ScreenId?.Refresh() ?? false;
            modified |= this.Users?.Refresh() ?? false;
            modified |= this.Title?.Refresh() ?? false;
            modified |= this.Instructions?.Refresh() ?? false;
            // TODO: below 2 lines might be causing excess updates
            modified |= this.UnityImages?.Refresh() ?? false;
            modified |= this.UnityImages?.Value?.Select(image => image?.Refresh() ?? false).ToList().Any(val => val) ?? false;
            return modified;
        }

        /// <summary>
        /// Tracks the first notification of a given UnityView;
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        private bool Dirty { get; set; } = true;

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public IAccessor<UnityViewOptions> Options { private get; set; }
        public UnityViewOptions _Options { get => Options?.Value; }
        public DateTime ServerTime { get { return DateTime.UtcNow; } }


        // TODO: this is pretty inefficient to be run as frequently as this gets run.
        public DateTime? _StateEndTime
        {
            get => this.Lobby?.GetAllUsers().SelectMany(user => user.StateStack).Select(state => state.ApproximateStateEndTime).Min();
        }

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public IAccessor<IReadOnlyList<UnityImage>> UnityImages { private get; set; }
        public IReadOnlyList<UnityImage> _UnityImages { get => UnityImages?.Value; }

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public IAccessor<TVScreenId> ScreenId { private get; set; }
        public TVScreenId? _ScreenId { get => ScreenId?.Value; }

        // TODO: Streamline what data is being sent about the user here
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public IAccessor<IReadOnlyList<User>> Users { private get; set; }
        public IReadOnlyList<User> _Users { get => Users?.Value; }

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public IAccessor<string> Title { private get; set; }
        public string _Title { get => Title?.Value; }

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public IAccessor<string> Instructions { private get; set; }
        public string _Instructions { get => Instructions?.Value; }
    }
}
