using RoystonGame.TV.DataModels.Users;
using System;
using System.Collections.Generic;
using RoystonGame.TV.GameModes.Common.DataModels;
using RoystonGame.Web.DataModels.UnityObjects;
using System.Drawing;

namespace RoystonGame.TV.GameModes.Common.ThreePartPeople.DataModels
{
    public class Person : UserCreatedUnityObject
    {

        public string Name { get; set; }
        public Dictionary<DrawingType, PeopleUserDrawing> BodyPartDrawings { get; set; } = new Dictionary<DrawingType, PeopleUserDrawing>();

        public enum DrawingType
        {
            Head, Body, Legs, None
        }

        public class PeopleUserDrawing : UserDrawing
        {
            public DrawingType Type { get; set; }
        }       
       
        public IReadOnlyList<string> GetOrderedDrawings()
        {
            return new List<string> { BodyPartDrawings[DrawingType.Head].Drawing, BodyPartDrawings[DrawingType.Body].Drawing, BodyPartDrawings[DrawingType.Legs].Drawing }.AsReadOnly();
        }
       
        public override UnityImage GetUnityImage(
            Color? backgroundColor = null,
            string imageIdentifier = null,
            Guid? imageOwnerId = null,
            string title = null,
            string header = null,
            int? voteCount = null,
            UnityImageVoteRevealOptions voteRevealOptions = null)
        {
            backgroundColor ??= Color.White;

            List<int> backgroundColorList = new List<int>
            {
                Convert.ToInt32(backgroundColor.Value.R),
                Convert.ToInt32(backgroundColor.Value.G),
                Convert.ToInt32(backgroundColor.Value.B)
            };

            return new UnityImage
            {
                Base64Pngs = new StaticAccessor<IReadOnlyList<string>> { Value = GetOrderedDrawings() },
                BackgroundColor = new StaticAccessor<IReadOnlyList<int>> { Value = backgroundColorList },
                SpriteGridWidth = new StaticAccessor<int?> { Value = 1 },
                SpriteGridHeight = new StaticAccessor<int?> { Value = 3 },
                ImageIdentifier = new StaticAccessor<string> { Value = imageIdentifier},
                ImageOwnerId = new StaticAccessor<Guid?> { Value = imageOwnerId},
                Title = new StaticAccessor<string> { Value = title },
                Header = new StaticAccessor<string> { Value = header },
                VoteCount = new StaticAccessor<int?> { Value = voteCount},
                VoteRevealOptions = new StaticAccessor<UnityImageVoteRevealOptions> { Value = voteRevealOptions },
            };
            
        }
    }
}
