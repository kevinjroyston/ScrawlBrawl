﻿using System;
using System.Collections.Generic;
using Backend.Games.Common.DataModels;
using Backend.APIs.DataModels.UnityObjects;
using System.Drawing;
using Backend.GameInfrastructure.DataModels.Users;
using Common.DataModels.Enums;
using Backend.Games.Common.GameStates.VoteAndReveal;
using Backend.GameInfrastructure.DataModels;

namespace Backend.Games.Common.ThreePartPeople.DataModels
{
    public class Person : UserCreatedUnityObject
    {
        public string Name { get; set; } = "N/A";

        // TODO: Create DrawingPerson and have both Text and Drawing inherit from this base.
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
                    Drawing = new DrawingObject(ThreePartPeopleConstants.Backgrounds[BodyPartType.Head])
                }
            },
            {
                BodyPartType.Body, new PeopleUserDrawing()
                {
                    Owner = new User("dummy"),
                    Type = BodyPartType.Body,
                    Drawing = new DrawingObject(ThreePartPeopleConstants.Backgrounds[BodyPartType.Body])
                }
            },
            {
                BodyPartType.Legs, new PeopleUserDrawing()
                {
                    Owner = new User("dummy"),
                    Type = BodyPartType.Legs,
                    Drawing = new DrawingObject(ThreePartPeopleConstants.Backgrounds[BodyPartType.Legs])
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
        public IReadOnlyList<DrawingObject> GetOrderedDrawings()
        {
            return new List<DrawingObject> { 
                BodyPartDrawings[BodyPartType.Head].Drawing, 
                BodyPartDrawings[BodyPartType.Body].Drawing,
                BodyPartDrawings[BodyPartType.Legs].Drawing 
            }.AsReadOnly();
        }
        public override UnityObject GetUnityObject(
            Color? backgroundColor = null,
            string imageIdentifier = null,
            Guid? imageOwnerId = null,
            string title = null,
            string header = null,
            int? voteCount = null,
            IReadOnlyList<Guid> usersWhoVotedFor = null,
            bool revealThisObject = false)
        {
            UnityImage baseImage = new UnityImage(base.GetUnityObject(backgroundColor, imageIdentifier, imageOwnerId, title, header, voteCount, usersWhoVotedFor, revealThisObject));
            baseImage.DrawingObjects = GetOrderedDrawings();
            baseImage.SpriteGridWidth = 1;
            baseImage.SpriteGridHeight = 3;
            return baseImage;
        }
    }
}
