using Backend.APIs.DataModels.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Backend.APIs.DataModels.UnityObjects
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
        public UnityObjectType Type { get; protected set; }
        public IReadOnlyList<Guid> UsersWhoVotedFor { get; set; }
        public UnityField<string> Title { get; set; }
        public UnityField<string> Header { get; set; }
        public UnityField<string> Footer { get; set; }
        public UnityField<string> ImageIdentifier { get; set; }
        public Guid? OwnerUserId { get; set; }
        public UnityField<int?> VoteCount { get; set; }
        public UnityField<IReadOnlyList<int>> BackgroundColor { get; set; }
        public Guid UnityObjectId { get; protected set; }
        public Dictionary<UnityObjectOptions, object> Options { get; set; }

        protected UnityObject(UnityObject other)
        {
            this.UsersWhoVotedFor = other.UsersWhoVotedFor;
            this.Type = other.Type;
            this.Title = other.Title;
            this.Header = other.Header;
            this.Footer = other.Footer;
            this.ImageIdentifier = other.ImageIdentifier;
            this.OwnerUserId = other.OwnerUserId;
            this.VoteCount = other.VoteCount;
            this.BackgroundColor = other.BackgroundColor;
            this.UnityObjectId = other.UnityObjectId;
            this.Options = other.Options;
        }
        protected UnityObject(Guid UnityObjectId)
        {
            this.UnityObjectId = UnityObjectId;
        }

        protected UnityObject()
        {
            this.UnityObjectId = Guid.NewGuid();
        }
    }
}
