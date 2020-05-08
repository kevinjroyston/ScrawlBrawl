using RoystonGame.TV.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RoystonGame.TV.GameModes.Common.DataModels;
using RoystonGame.Web.DataModels.UnityObjects;

namespace RoystonGame.TV.GameModes.Common.ThreePartPeople.DataModels
{
    public abstract class Person
    {
        /// <summary>
        /// The user who came up with this challenge
        /// </summary>
        public User Owner { get; set; }
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        protected virtual string DisplayName { get { return Name; } }
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
       
        public UnityImage GetPersonImaage()
        {
            
            return new UnityImage
            {
                Base64Pngs = new StaticAccessor<IReadOnlyList<string>> { Value = GetOrderedDrawings() },
                BackgroundColor = new StaticAccessor<IReadOnlyList<int>> { Value = new List<int> { 255, 255, 255 } },
                SpriteGridWidth = new StaticAccessor<int?> { Value = 1 },
                SpriteGridHeight = new StaticAccessor<int?> { Value = 3 },
                Header = new StaticAccessor<string> { Value = DisplayName }
            };
            
        }
    }
}
