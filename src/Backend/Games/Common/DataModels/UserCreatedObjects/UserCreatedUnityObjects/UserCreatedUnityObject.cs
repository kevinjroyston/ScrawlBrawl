using Backend.APIs.DataModels.UnityObjects;
using System;
using System.Drawing;

namespace Backend.Games.Common.DataModels
{
    public abstract class UserCreatedUnityObject : UserCreatedObject
    {
        public UserCreatedUnityObject() : base()
        {
            //empty
        }
        public abstract UnityImage GetUnityImage(
            Color? backgroundColor = null,
            string imageIdentifier = null,
            Guid? imageOwnerId = null,
            string title = null,
            string header = null,
            int? voteCount = null,
            UnityImageVoteRevealOptions voteRevealOptions = null);
    }
}
