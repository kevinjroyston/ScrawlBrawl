using Backend.APIs.DataModels.Enums;
using Backend.APIs.DataModels.UnityObjects;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Backend.Games.Common.DataModels
{
    public class UserText : UserCreatedUnityObject
    {
        public string Text { get; set; }
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
            UnityText baseText = new UnityText(base.GetUnityObject(backgroundColor, imageIdentifier, imageOwnerId, title, header, voteCount, usersWhoVotedFor, revealThisObject));
            return baseText;
        }
    }
}
