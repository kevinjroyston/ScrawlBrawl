using Backend.APIs.DataModels.UnityObjects;
using Backend.GameInfrastructure.DataModels.Users;
using Backend.Games.Common.GameStates.VoteAndReveal;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Backend.Games.Common.DataModels
{
    public abstract class UserCreatedUnityObject : UserCreatedObject, IVotable
    {
        public UserCreatedUnityObject() : base()
        {
            //empty
        }

        public List<VoteInfo> VotesCastForThisObject { get; } = new List<VoteInfo>();

        public bool ShouldHighlightReveal { get; set; } = false;
        public UnityObjectOverrides UnityImageRevealOverrides { get; set; }
        public UnityObjectOverrides UnityImageVotingOverrides { get; set; }

        public virtual Legacy_UnityImage GetUnityImage(
            Color? backgroundColor = null,
            string imageIdentifier = null,
            Guid? imageOwnerId = null,
            string title = null,
            string header = null,
            int? voteCount = null,
            Legacy_UnityImageVoteRevealOptions voteRevealOptions = null)
        {
            backgroundColor ??= Color.White;

            List<int> backgroundColorList = new List<int>
            {
                Convert.ToInt32(backgroundColor.Value.R),
                Convert.ToInt32(backgroundColor.Value.G),
                Convert.ToInt32(backgroundColor.Value.B)
            };

            return new Legacy_UnityImage
            {
                BackgroundColor = new StaticAccessor<IReadOnlyList<int>> { Value = backgroundColorList },
                SpriteGridWidth = new StaticAccessor<int?> { Value = 1 },
                SpriteGridHeight = new StaticAccessor<int?> { Value = 1 },
                ImageIdentifier = new StaticAccessor<string> { Value = imageIdentifier },
                ImageOwnerId = new StaticAccessor<Guid?> { Value = imageOwnerId },
                Title = new StaticAccessor<string> { Value = title },
                Header = new StaticAccessor<string> { Value = header },
                VoteCount = new StaticAccessor<int?> { Value = voteCount },
                VoteRevealOptions = new StaticAccessor<Legacy_UnityImageVoteRevealOptions> { Value = voteRevealOptions },
            };

        }
        public Legacy_UnityImage VotingUnityObjectGenerator(int numericId)
        {
            return GetUnityImage(
                imageIdentifier: numericId.ToString(),
                title: this.UnityImageVotingOverrides.Title,
                header: this.UnityImageVotingOverrides.Header);
        }
        public Legacy_UnityImage RevealUnityObjectGenerator(int numericId)
        {
            return GetUnityImage(
                imageIdentifier: numericId.ToString(),
                title: this.UnityImageRevealOverrides.Title,
                header: this.UnityImageRevealOverrides.Header,
                imageOwnerId: this.Owner?.Id,
                voteRevealOptions: new Legacy_UnityImageVoteRevealOptions()
                {
                    RelevantUsers = new StaticAccessor<IReadOnlyList<User>> { Value = this.VotesCastForThisObject.Select((vote) => vote.UserWhoVoted).ToList() },
                    RevealThisImage = new StaticAccessor<bool?> { Value = this.ShouldHighlightReveal }
                });
        }
    }
}
