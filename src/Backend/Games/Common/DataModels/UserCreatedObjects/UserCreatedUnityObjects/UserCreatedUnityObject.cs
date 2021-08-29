using Backend.APIs.DataModels.Enums;
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

        public virtual UnityObject GetUnityObject(
            Color? backgroundColor = null,
            string imageIdentifier = null,
            Guid? imageOwnerId = null,
            string title = null,
            string header = null,
            int? voteCount = null,
            IReadOnlyList<Guid> usersWhoVotedFor = null,
            bool revealThisObject = false)
        {
            backgroundColor ??= Color.White;

            List<int> backgroundColorList = new List<int>
            {
                Convert.ToInt32(backgroundColor.Value.R),
                Convert.ToInt32(backgroundColor.Value.G),
                Convert.ToInt32(backgroundColor.Value.B)
            };

            // This pattern is pretty bad. Should be returning some kind of generic UnityObject which children can turn into whatever type is needed (and add more fields).
            // instead using a questionable copy constructor pattern.
            return new UnityImage
            {
                BackgroundColor = new UnityField<IReadOnlyList<int>> { Value = backgroundColorList },
                SpriteGridWidth = 1,
                SpriteGridHeight = 1,
                ImageIdentifier = new UnityField<string> { Value = imageIdentifier },
                OwnerUserId = imageOwnerId,
                Title = new UnityField<string> { Value = title },
                Header = new UnityField<string> { Value = header },
                VoteCount = new UnityField<int?> { Value = voteCount },
                UsersWhoVotedFor = usersWhoVotedFor,
                Options = new Dictionary<UnityObjectOptions, object> {
                    { UnityObjectOptions.RevealThisImage, revealThisObject}
                }
            };

        }
        public UnityObject VotingUnityObjectGenerator(int numericId)
        {
            return GetUnityObject(
                imageIdentifier: numericId.ToString(),
                title: this.UnityImageVotingOverrides.Title,
                header: this.UnityImageVotingOverrides.Header);
        }
        public UnityObject RevealUnityObjectGenerator(int numericId)
        {
            return GetUnityObject(
                imageIdentifier: numericId.ToString(),
                title: this.UnityImageRevealOverrides.Title,
                header: this.UnityImageRevealOverrides.Header,
                imageOwnerId: this.Owner?.Id,
                usersWhoVotedFor: this.VotesCastForThisObject.Select((vote) => vote.UserWhoVoted.Id).ToList(),
                revealThisObject: this.ShouldHighlightReveal);
        }
    }
}
