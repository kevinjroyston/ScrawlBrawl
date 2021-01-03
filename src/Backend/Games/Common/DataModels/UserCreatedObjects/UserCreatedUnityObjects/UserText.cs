using Backend.APIs.DataModels.UnityObjects;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Backend.Games.Common.DataModels
{
    public class UserText : UserCreatedUnityObject
    {
        public string Text { get; set; }
        public override Legacy_UnityImage GetUnityImage(
            Color? backgroundColor = null,
            string imageIdentifier = null,
            Guid? imageOwnerId = null,
            string title = null,
            string header = null,
            int? voteCount = null,
            Legacy_UnityImageVoteRevealOptions voteRevealOptions = null)
        {
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
    }
}
