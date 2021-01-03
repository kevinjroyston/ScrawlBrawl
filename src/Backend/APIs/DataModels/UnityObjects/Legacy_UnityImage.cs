using System;
using System.Collections.Generic;

namespace Backend.APIs.DataModels.UnityObjects
{
    public class Legacy_UnityImage : IAccessorHashable
    {
        public Legacy_UnityImage (Guid UnityImageId)
        {
            this._UnityImageId = UnityImageId;
        }
        public Legacy_UnityImage()
        {
            
        }
        public bool Refresh()
        {
            bool modified = false;
            modified |= this.Base64Pngs?.Refresh() ?? false;
            modified |= this.Title?.Refresh() ?? false;
            modified |= this.Header?.Refresh() ?? false;
            modified |= this.Footer?.Refresh() ?? false;
            modified |= this.VoteCount?.Refresh() ?? false;
            modified |= this.BackgroundColor?.Refresh() ?? false;
            modified |= this.ImageIdentifier?.Refresh() ?? false;
            modified |= this.SpriteGridWidth?.Refresh() ?? false;
            modified |= this.SpriteGridHeight?.Refresh() ?? false;
            modified |= this.ImageOwnerId?.Refresh() ?? false;
            modified |= this.VoteRevealOptions?.Refresh() ?? false;
            return modified;
        }

        public int GetIAccessorHashCode()
        {
            var hash = new HashCode();
            foreach(string png in _Base64Pngs ?? new List<string>())
            {
                hash.Add(png);
            }
            hash.Add(_Title);
            hash.Add(_SpriteGridWidth);
            hash.Add(_SpriteGridHeight);
            hash.Add(_Header);
            hash.Add(_Footer);
            hash.Add(_VoteCount);
            hash.Add(_ImageIdentifier);
            hash.Add(_ImageOwnerId);
            hash.Add(_VoteRevealOptions);
            foreach(int i in _BackgroundColor ?? new List<int>())
            {
                hash.Add(i);
            }
            return hash.ToHashCode();
        }

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public IAccessor<IReadOnlyList<string>> Base64Pngs { get; set; }
        public IReadOnlyList<string> _Base64Pngs { get => Base64Pngs?.Value; }

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public IAccessor<string> Title { get; set; }
        public string _Title { get => Title?.Value; }

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public IAccessor<int?> SpriteGridWidth { get; set; }
        public int? _SpriteGridWidth { get => SpriteGridWidth?.Value; }

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public IAccessor<int?> SpriteGridHeight { get; set; }
        public int? _SpriteGridHeight { get => SpriteGridHeight?.Value; }

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public IAccessor<string> Header { get; set; }
        public string _Header { get => Header?.Value; }

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public IAccessor<string> Footer { get; set; }
        public string _Footer { get => Footer?.Value; }

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public IAccessor<int?> VoteCount { get; set; }
        public int? _VoteCount { get => VoteCount?.Value; }

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public IAccessor<string> ImageIdentifier { get; set; }
        public string _ImageIdentifier { get => ImageIdentifier?.Value; }

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public IAccessor<Guid?> ImageOwnerId { get; set; }
        public Guid? _ImageOwnerId { get => ImageOwnerId?.Value; }

        public Guid _UnityImageId { get; } = Guid.NewGuid();

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public IAccessor<Legacy_UnityImageVoteRevealOptions> VoteRevealOptions { get; set; }
        public Legacy_UnityImageVoteRevealOptions _VoteRevealOptions { get => VoteRevealOptions?.Value; }

        // TODO: add some validation to this setter.
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public IAccessor<IReadOnlyList<int>> BackgroundColor { get; set; }
        public IReadOnlyList<int> _BackgroundColor { get => BackgroundColor?.Value; }
    }
}
