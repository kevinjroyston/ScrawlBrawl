using Backend.APIs.DataModels.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Backend.APIs.DataModels.UnityObjects
{
    public enum UnityObjectType
    {
        Image,
        Text,
        String,
        Slider,
    }

    public class UnityObject : IAccessorHashable
    {
        public UnityObject(Legacy_UnityImage legacy)
        {
            this.Base64Pngs = legacy.Base64Pngs;
            this.UsersWhoVotedFor = new StaticAccessor<IReadOnlyList<Guid>> 
            { 
                Value = legacy._VoteRevealOptions?._RelevantUsers?.Select(user => user.Id).ToList() 
            };
            // this.Type TODO
            this.SpriteGridWidth = legacy.SpriteGridWidth;
            this.SpriteGridHeight = legacy.SpriteGridHeight;
            this.Title = new StaticAccessor<UnityField<string>>
            {
                Value = new UnityField<string>
                {
                    Value = legacy._Title
                }
            };
            this.Header = new StaticAccessor<UnityField<string>>
            {
                Value = new UnityField<string>
                {
                    Value = legacy._Header
                }
            };
            this.Footer = new StaticAccessor<UnityField<string>>
            {
                Value = new UnityField<string>
                {
                    Value = legacy._Footer
                }
            };
            this.ImageIdentifier = new StaticAccessor<UnityField<string>>
            {
                Value = new UnityField<string>
                {
                    Value = legacy._ImageIdentifier
                }
            };
            this.ImageOwnerId = legacy.ImageOwnerId;
            this.VoteCount = new StaticAccessor<UnityField<int?>>
            {
                Value = new UnityField<int?>
                {
                    Value = legacy._VoteCount
                }
            };
            this.BackgroundColor = new StaticAccessor<UnityField<IReadOnlyList<int>>>
            {
                Value = new UnityField<IReadOnlyList<int>>
                {
                    Value = legacy._BackgroundColor
                }
            };
            this.Options
        }

        public int GetIAccessorHashCode()
        {
            var hash = new HashCode();
            foreach (string png in _Base64Pngs ?? new List<string>())
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
            hash.Add(_Options);
            return hash.ToHashCode();
        }
        public bool Refresh()
        {
            bool modified = false;
            modified |= this.UsersWhoVotedFor?.Refresh() ?? false;
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
            modified |= this.Options?.Refresh() ?? false;
            return modified;
        }

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public IAccessor<IReadOnlyList<string>> Base64Pngs { private get; set; }
        public IReadOnlyList<string> _Base64Pngs { get => Base64Pngs?.Value; }

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public IAccessor<IReadOnlyList<Guid>> UsersWhoVotedFor { private get; set; }
        public IReadOnlyList<Guid> _UsersWhoVotedFor { get => UsersWhoVotedFor?.Value; }

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public IAccessor<UnityObjectType> Type { private get; set; }
        public UnityObjectType _Type { get => Type?.Value ?? UnityObjectType.Text; }

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public IAccessor<int?> SpriteGridWidth { private get; set; }
        public int? _SpriteGridWidth { get => SpriteGridWidth?.Value; }

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public IAccessor<int?> SpriteGridHeight { private get; set; }
        public int? _SpriteGridHeight { get => SpriteGridHeight?.Value; }

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public IAccessor<UnityField<string>> Title { private get; set; }
        public UnityField<string> _Title { get => Title?.Value; }

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public IAccessor<UnityField<string>> Header { private get; set; }
        public UnityField<string> _Header { get => Header?.Value; }

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public IAccessor<UnityField<string>> Footer { private get; set; }
        public UnityField<string> _Footer { get => Footer?.Value; }

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public IAccessor<UnityField<string>> ImageIdentifier { private get; set; }
        public UnityField<string> _ImageIdentifier { get => ImageIdentifier?.Value; }

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public IAccessor<Guid?> ImageOwnerId { private get; set; }
        public Guid? _ImageOwnerId { get => ImageOwnerId?.Value; }

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public IAccessor<UnityField<int?>> VoteCount { private get; set; }
        public UnityField<int?> _VoteCount { get => VoteCount?.Value; }

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public IAccessor<UnityField<IReadOnlyList<int>>> BackgroundColor { private get; set; }
        public UnityField<IReadOnlyList<int>> _BackgroundColor { get => BackgroundColor?.Value; }

        public Guid _UnityObjectId { get; } = Guid.NewGuid();


        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public IAccessor<Dictionary<UnityObjectOptions, object>> Options { private get; set; }
        public Dictionary<UnityObjectOptions, object> _Options { get => Options?.Value; }
    }
}
