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
        public Guid UnityObjectId { get; private set; }
        public Dictionary<UnityObjectOptions, object> Options { get; set; }

        protected UnityObject(Legacy_UnityImage legacy)
        {
            this.UsersWhoVotedFor = legacy._VoteRevealOptions?._RelevantUsers?.Select(user => user.Id).ToList().AsReadOnly();
            this.Title = new UnityField<string> { Value = legacy._Title };
            this.Header = new UnityField<string> { Value = legacy._Header };
            this.Footer = new UnityField<string> { Value = legacy._Footer };
            this.ImageIdentifier = new UnityField<string> { Value = legacy._ImageIdentifier };
            this.OwnerUserId = legacy._ImageOwnerId;
            this.VoteCount = new UnityField<int?> { Value = legacy._VoteCount };
            this.BackgroundColor = new UnityField<IReadOnlyList<int>> { Value = legacy._BackgroundColor };
            this.UnityObjectId = legacy._UnityImageId;
            this.Options = new Dictionary<UnityObjectOptions, object>
            {
                 {UnityObjectOptions.RevealThisImage, legacy._VoteRevealOptions?._RevealThisImage }
            };
        }
        protected UnityObject()
        {
            this.UnityObjectId = Guid.NewGuid();
        }
    }
}
