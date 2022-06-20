using Backend.APIs.DataModels.UnityObjects;
using System;
using System.Drawing;
using System.Collections.Generic;
using Backend.Games.Common.GameStates.VoteAndReveal;
using Backend.GameInfrastructure.DataModels.Users;
using System.Linq;
using Backend.GameInfrastructure.DataModels;

namespace Backend.Games.Common.DataModels
{
    public class UserDrawing : UserCreatedUnityObject
    {
        public UserDrawing() : base()
        {
            //empty
        }
        public DrawingObject Drawing { get; set; }

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
            baseImage.DrawingObjects = new List<DrawingObject>() { this.Drawing }.AsReadOnly();
            return baseImage;
        }
    }
}
