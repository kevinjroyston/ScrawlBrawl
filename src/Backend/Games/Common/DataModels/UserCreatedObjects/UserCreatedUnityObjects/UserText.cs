using Backend.APIs.DataModels.Enums;
using Backend.APIs.DataModels.UnityObjects;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Backend.Games.Common.DataModels
{
    public class UserText : UserCreatedUnityObject
    {
        public string Text { get; set; }
        public override UnityImage GetUnityImage(
            Color? backgroundColor = null,
            string imageIdentifier = null,
            Guid? imageOwnerId = null,
            string title = null,
            string header = null,
            int? voteCount = null,
            IReadOnlyList<Guid> usersWhoVotedFor = null,
            bool revealThisObject = false)
        {
            List<int> backgroundColorList = new List<int>
            {
                Convert.ToInt32(backgroundColor.Value.R),
                Convert.ToInt32(backgroundColor.Value.G),
                Convert.ToInt32(backgroundColor.Value.B)
            };

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
                Options = new Dictionary<UnityObjectOptions, object>
                {
                     {UnityObjectOptions.RevealThisImage, revealThisObject }
                }
            };
        }
    }
}
