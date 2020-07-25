using RoystonGame.TV.DataModels.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.Web.DataModels.UnityObjects
{
    public class UnityImageVoteRevealOptions
    {
        public bool Refresh()
        {
            bool modified = false;
            modified |= this.RelevantUsers?.Refresh() ?? false;
            modified |= this.ImageOwner?.Refresh() ?? false;
            return modified;
        }

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public IAccessor<IReadOnlyList<User>> RelevantUsers { private get; set; }
        public IReadOnlyList<User> _RelevantUsers { get => RelevantUsers?.Value; }

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public IAccessor<User> ImageOwner { private get; set; }
        public User _ImageOwner { get => ImageOwner?.Value; }

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public IAccessor<bool?> RevealThisImage { private get; set; }
        public bool? _RevealThisImage { get => RevealThisImage?.Value; }
    }
}
