using RoystonGame.TV.DataModels.Users;
using RoystonGame.Web.DataModels.UnityObjects;
using System;
using System.Drawing;
using System.Collections.Generic;

namespace RoystonGame.TV.GameModes.Common.DataModels
{
    public class UserDrawing
    {
        public string Drawing { get; set; }
        public Guid Id { get; set; }
        public User Owner { get; set; }
        #region UnityFields
        protected virtual string UnityImageIdentifier { get { return null; } }
        protected virtual string UnityImageTitle { get { return null; } }
        protected virtual string UnityImageHeader { get { return null; } }
        protected virtual Color? UnityImageBackGroundColor { get { return Color.White; } }
        #endregion
        public UnityImage GetUnityImage(
            Color? backgroundColor = null,
            string imageIdentifier = null,
            string title = null,
            string header = null,
            int? voteCount = null,
            UnityImageVoteRevealOptions voteRevealOptions = null)
        {
            backgroundColor = backgroundColor ?? UnityImageBackGroundColor;
            imageIdentifier = imageIdentifier ?? UnityImageIdentifier;
            title = title ?? UnityImageTitle;
            header = header ?? UnityImageHeader;
            List<int> backgroundColorList = new List<int>
            {
                Convert.ToInt32(backgroundColor.Value.R),
                Convert.ToInt32(backgroundColor.Value.G),
                Convert.ToInt32(backgroundColor.Value.B)
            };

            return new UnityImage
            {
                Base64Pngs = new StaticAccessor<IReadOnlyList<string>> { Value = new List<string>() { this.Drawing }.AsReadOnly() },
                BackgroundColor = new StaticAccessor<IReadOnlyList<int>> { Value = backgroundColorList },
                SpriteGridWidth = new StaticAccessor<int?> { Value = 1 },
                SpriteGridHeight = new StaticAccessor<int?> { Value = 1 },
                ImageIdentifier = new StaticAccessor<string> { Value = imageIdentifier },
                Title = new StaticAccessor<string> { Value = title },
                Header = new StaticAccessor<string> { Value = header },
                VoteCount = new StaticAccessor<int?> { Value = voteCount},
                VoteRevealOptions = new StaticAccessor<UnityImageVoteRevealOptions> { Value = voteRevealOptions},
            };

        }
    }
}
