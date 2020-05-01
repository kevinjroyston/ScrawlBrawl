using RoystonGame.TV.DataModels;
using System.Collections.Generic;

namespace RoystonGame.Web.DataModels.UnityObjects
{
    public class UnityImage
    {
        public bool Refresh()
        {
            bool modified = false;
            modified |= this.Base64Pngs?.Refresh() ?? false;
            modified |= this.Title?.Refresh() ?? false;
            modified |= this.Header?.Refresh() ?? false;
            modified |= this.Footer?.Refresh() ?? false;
            modified |= this.VoteCount?.Refresh() ?? false;
            modified |= this.RelevantUsers?.Refresh() ?? false;
            modified |= this.BackgroundColor?.Refresh() ?? false;
            modified |= this.ImageIdentifier?.Refresh() ?? false;
            return modified;
        }

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public IAccessor<IReadOnlyList<string>> Base64Pngs { private get; set; }
        public IReadOnlyList<string> _Base64Pngs { get => Base64Pngs?.Value; }

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public IAccessor<string> Title { private get; set; }
        public string _Title { get => Title?.Value; }

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public IAccessor<string> Header { private get; set; }
        public string _Header { get => Header?.Value; }

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public IAccessor<string> Footer { private get; set; }
        public string _Footer { get => Footer?.Value; }

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public IAccessor<int?> VoteCount { private get; set; }
        public int? _VoteCount { get => VoteCount?.Value; }

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public IAccessor<string> ImageIdentifier { private get; set; }
        public string _ImageIdentifier { get => ImageIdentifier?.Value; }

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public IAccessor<IReadOnlyList<User>> RelevantUsers { private get; set; }
        public IReadOnlyList<User> _RelevantUsers { get => RelevantUsers?.Value; }

        // TODO: add some validation to this setter.
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public IAccessor<IReadOnlyList<int>> BackgroundColor { private get; set; }
        public IReadOnlyList<int> _BackgroundColor { get => BackgroundColor?.Value; }
    }
}
