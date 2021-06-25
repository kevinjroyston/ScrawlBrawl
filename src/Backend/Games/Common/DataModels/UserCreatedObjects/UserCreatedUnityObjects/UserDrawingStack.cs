using Backend.APIs.DataModels.UnityObjects;
using Backend.Games.Common.GameStates.VoteAndReveal;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Games.Common.DataModels.UserCreatedObjects.UserCreatedUnityObjects
{
    public class UserDrawingStack<T>:UserCreatedUnityObject, IVotable where T:UserDrawing
    {
        public List<T> UserDrawings { get; set; }

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
            UnityImage baseImage = base.GetUnityImage(backgroundColor, imageIdentifier, imageOwnerId, title, header, voteCount, usersWhoVotedFor, revealThisObject);
            baseImage.Base64Pngs = UserDrawings.Select(drawing=>drawing.Drawing).ToList().AsReadOnly();
            return baseImage;
        }
    }
}
