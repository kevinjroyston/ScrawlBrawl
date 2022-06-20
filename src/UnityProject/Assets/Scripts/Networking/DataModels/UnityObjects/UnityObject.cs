using Assets.Scripts.Networking.DataModels.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Networking.DataModels.UnityObjects
{
    public enum UnityObjectType
    {
        // DO NOT CHANGE THE ORDER HERE WITHOUT UPDATING CODE. Default matters during deserialization
        Image = 0,
        Text,
        String,
        Slider,
    }
    public abstract class UnityObject : OptionsInterface<UnityObjectOptions>
    {
        public List<Guid> UsersWhoVotedFor { get; set; }
        public UnityObjectType Type { get; set; }
        public UnityField<string> Title { get; set; }
        public UnityField<string> Header { get; set; }
        public UnityField<string> Footer { get; set; }
        public UnityField<string> ImageIdentifier { get; set; }
        public Guid? OwnerUserId { get; set; }
        public UnityField<int?> VoteCount { get; set; }
        public UnityField<List<int>> BackgroundColor { get; set; }
        public Guid UnityObjectId { get; set; }
        public Dictionary<UnityObjectOptions, object> Options { get; set; }
    }
}
