using RoystonGame.TV.DataModels.Users;
using RoystonGame.Web.DataModels.UnityObjects;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.TV.GameModes.Common.DataModels
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
