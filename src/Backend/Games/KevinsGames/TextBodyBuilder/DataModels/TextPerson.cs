using Backend.APIs.DataModels.UnityObjects;
using Backend.GameInfrastructure.DataModels.Users;
using Backend.Games.Common.DataModels;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Games.KevinsGames.TextBodyBuilder.DataModels
{
    public class TextPerson : UserCreatedUnityObject
    {
        public string Character { get; set; } = "N/A";
        public string Action { get; set; } = "N/A";
        public string Modifier { get; set; } = "N/A";

        public TextPerson()
        {
            // TODO: not the most ideal way to make dummy users. Should use an interface and a new DummyUser class
            this.Owner = new User("dummy");
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
            UnityText text = new UnityText(base.GetUnityObject(backgroundColor, imageIdentifier, imageOwnerId, title, header, voteCount, usersWhoVotedFor, revealThisObject));
            text.Title ??= new UnityField<string> { Value = Character };
            text.Header ??= new UnityField<string> { Value = $"{Action} {Modifier}" };
            return text;
        }
    }
}
