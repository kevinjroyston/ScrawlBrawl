using RoystonGame.TV.DataModels.Users;
using RoystonGame.Web.DataModels.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RoystonGame.Web.DataModels.UnityObjects
{
    public class UnityView
    {
        public bool Refresh()
        {
            // First Refresh is always dirty.
            bool modified = this.Dirty;
            this.Dirty = false;

            modified |= this.Options?.Refresh() ?? false;
            modified |= this.ScreenId?.Refresh() ?? false;
            modified |= this.StateEndTime?.Refresh() ?? false;
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
        public bool Dirty { get; set; } = true;

        public UnityViewOptions Options { get; set; }

        public DateTime ServerTime { get { return DateTime.UtcNow; } }


        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public IAccessor<DateTime> StateEndTime { private get; set; }
        public DateTime? _StateEndTime { get => StateEndTime?.Value; }

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
