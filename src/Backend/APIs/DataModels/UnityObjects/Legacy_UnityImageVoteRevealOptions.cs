using Backend.GameInfrastructure.DataModels.Users;
using System.Collections.Generic;

namespace Backend.APIs.DataModels.UnityObjects
{
    public class Legacy_UnityImageVoteRevealOptions
    {
        public bool Refresh()
        {
            bool modified = false;
            modified |= this.RelevantUsers?.Refresh() ?? false;
            return modified;
        }

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public IAccessor<IReadOnlyList<User>> RelevantUsers { private get; set; }
        public IReadOnlyList<User> _RelevantUsers { get => RelevantUsers?.Value; }

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public IAccessor<bool?> RevealThisImage { private get; set; }
        public bool? _RevealThisImage { get => RevealThisImage?.Value; }
    }
}
