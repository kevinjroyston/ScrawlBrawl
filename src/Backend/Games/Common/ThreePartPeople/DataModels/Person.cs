using System;
using System.Collections.Generic;
using Backend.Games.Common.DataModels;
using Backend.APIs.DataModels.UnityObjects;
using System.Drawing;
using Backend.GameInfrastructure.DataModels.Users;
using Common.DataModels.Enums;

namespace Backend.Games.Common.ThreePartPeople.DataModels
{
    public class Person : UserCreatedUnityObject
    {

        public string Name { get; set; } = "N/A";

        public Person()
        {
            // TODO: not the most ideal way to make dummy users. Should use an interface and a new DummyUser class
            this.Owner = new User("dummy");
        }

        /// <summary>
        /// Initialize to defaults, TODO: improve this.
        /// </summary>
        public Dictionary<BodyPartType, PeopleUserDrawing> BodyPartDrawings { get; set; } = new Dictionary<BodyPartType, PeopleUserDrawing>()
        {
            {
                BodyPartType.Head, new PeopleUserDrawing()
                {
                    Owner = new User("dummy"),
                    Type = BodyPartType.Head,
                    Drawing = ThreePartPeopleConstants.Backgrounds[BodyPartType.Head]
                }
            },
            {
                BodyPartType.Body, new PeopleUserDrawing()
                {
                    Owner = new User("dummy"),
                    Type = BodyPartType.Body,
                    Drawing = ThreePartPeopleConstants.Backgrounds[BodyPartType.Body]
                }
            },
            {
                BodyPartType.Legs, new PeopleUserDrawing()
                {
                    Owner = new User("dummy"),
                    Type = BodyPartType.Legs,
                    Drawing = ThreePartPeopleConstants.Backgrounds[BodyPartType.Legs]
                }
            }
        };

        public enum BodyPartType
        {
            Head, Body, Legs, None
        }

        public class PeopleUserDrawing : UserDrawing
        {
            public BodyPartType Type { get; set; }
        }
        public IReadOnlyList<string> GetOrderedDrawings()
        {
            return new List<string> { BodyPartDrawings[BodyPartType.Head].Drawing, BodyPartDrawings[BodyPartType.Body].Drawing, BodyPartDrawings[BodyPartType.Legs].Drawing }.AsReadOnly();
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
