using Assets.Scripts.Networking.DataModels.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Networking.DataModels
{
    public enum UnityObjectType
    {
        Image,
        Text,
        String,
        Slider,
    }
    public class UnityObject : OptionsInterface<UnityObjectOptions>
    {
        public List<Guid> UsersWhoVotedFor { get; set; }
        public UnityObjectType Type { get; set; }
        public IReadOnlyList<Sprite> Sprites { get; set; }
        public int? SpriteGridWidth { get; set; }
        public int? SpriteGridHeight { get; set; }
        public UnityField<string> Title { get; set; }
        public UnityField<string> Header { get; set; }
        public UnityField<string> Footer { get; set; }
        public UnityField<string> ImageIdentifier { get; set; }
        public Guid? ImageOwnerId { get; set; }
        public UnityField<int?> VoteCount { get; set; }
        public UnityField<IReadOnlyList<int>> BackgroundColor { get; set; }
        public Guid UnityObjectId { get; set; }
        public Dictionary<UnityObjectOptions, object> Options { get; set; }
    }
}
