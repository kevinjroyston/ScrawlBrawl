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

        public override Legacy_UnityImage GetUnityImage(
            Color? backgroundColor = null,
            string imageIdentifier = null,
            Guid? imageOwnerId = null,
            string title = null,
            string header = null,
            int? voteCount = null,
            Legacy_UnityImageVoteRevealOptions voteRevealOptions = null)
        {
            Legacy_UnityImage baseImage = base.GetUnityImage(backgroundColor, imageIdentifier, imageOwnerId, title, header, voteCount, voteRevealOptions);
            baseImage.Base64Pngs = new StaticAccessor<IReadOnlyList<string>> { Value = UserDrawings.Select(drawing=>drawing.Drawing).ToList().AsReadOnly() };
            return baseImage;
        }
    }
}
