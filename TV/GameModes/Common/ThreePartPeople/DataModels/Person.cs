using RoystonGame.TV.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RoystonGame.TV.GameModes.Common.DataModels;
using RoystonGame.Web.DataModels.UnityObjects;
using System.Drawing;

namespace RoystonGame.TV.GameModes.Common.ThreePartPeople.DataModels
{
    public class Person
    {
        /// <summary>
        /// The user who came up with this challenge
        /// </summary>
        public User Owner { get; set; }
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        #region UnityFields
        protected virtual string UnityImageIdentifier { get { return null; } }
        protected virtual string UnityImageTitle { get { return Name; } }
        protected virtual string UnityImageHeader { get { return null; } }
        protected virtual Color? UnityImageBackGroundColor { get { return Color.White; } }
        #endregion
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
       
        public UnityImage GetPersonImage(
            Color? backgroundColor = null,
            string imageIdentifier = null,
            string title = null,
            string header = null)
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
                Base64Pngs = new StaticAccessor<IReadOnlyList<string>> { Value = GetOrderedDrawings() },
                BackgroundColor = new StaticAccessor<IReadOnlyList<int>> { Value = backgroundColorList },
                SpriteGridWidth = new StaticAccessor<int?> { Value = 1 },
                SpriteGridHeight = new StaticAccessor<int?> { Value = 3 },
                ImageIdentifier = new StaticAccessor<string> { Value = imageIdentifier},
                Title = new StaticAccessor<string> { Value = title },
                Header = new StaticAccessor<string> { Value = header },
            };
            
        }
    }
}
