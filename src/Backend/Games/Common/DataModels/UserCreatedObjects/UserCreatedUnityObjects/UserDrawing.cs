using Backend.APIs.DataModels.UnityObjects;
using System;
using System.Drawing;
using System.Collections.Generic;
using Backend.Games.Common.GameStates.VoteAndReveal;
using Backend.GameInfrastructure.DataModels.Users;
using System.Linq;

namespace Backend.Games.Common.DataModels
{
    public class UserDrawing : UserCreatedUnityObject
    {
        public UserDrawing() : base()
        {
            //empty
        }
        public string Drawing { get; set; }

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
            baseImage.Base64Pngs = new StaticAccessor<IReadOnlyList<string>> { Value = new List<string>() { this.Drawing }.AsReadOnly() };
            return baseImage;
        }
    }
}
