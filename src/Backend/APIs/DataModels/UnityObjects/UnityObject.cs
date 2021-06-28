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


    public abstract class UnityObject : OptionsInterface<UnityObjectOptions>
    {
        public IReadOnlyList<Guid> UsersWhoVotedFor { get; set; }
        public UnityObjectType Type { get; protected set; }
        public UnityField<string> Title { get; set; }
        public UnityField<string> Header { get; set; }
        public UnityField<string> Footer { get; set; }
        public UnityField<string> ImageIdentifier { get; set; }
        public Guid? OwnerUserId { get; set; }
        public UnityField<int?> VoteCount { get; set; }
        public UnityField<IReadOnlyList<int>> BackgroundColor { get; set; }
        public Guid UnityObjectId { get; protected set; }
        public Dictionary<UnityObjectOptions, object> Options { get; set; }

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
