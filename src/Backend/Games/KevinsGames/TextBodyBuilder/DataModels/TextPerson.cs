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
        /// <summary>
        /// Initialize to defaults
        /// </summary>
        public Dictionary<CAMType, CAMUserText> Descriptors { get; set; } = new Dictionary<CAMType, CAMUserText>()
        {
            {
                CAMType.Character, new CAMUserText()
                {
                    Owner = new User("dummy"),
                    Type =CAMType.Character,
                    Text = "N/A",
                }
            },
            {
                CAMType.Action, new CAMUserText()
                {
                    Owner = new User("dummy"),
                    Type =CAMType.Action,
                    Text = "N/A",
                }
            },
            {
                CAMType.Modifier, new CAMUserText()
                {
                    Owner = new User("dummy"),
                    Type =CAMType.Modifier,
                    Text = "N/A",
                }
            }
        };

        public TextPerson()
        {
            // TODO: not the most ideal way to make dummy users. Should use an interface and a new DummyUser class
            this.Owner = new User("dummy");
        }
        /// <summary>
        /// This wont directly be turned into a unity object as it only contains info for one CAM
        /// </summary>
        public class CAMUserText : UserText
        {
            public CAMType Type { get; set; }
        }
        public enum CAMType
        {
            None,
            Character,
            Action,
            Modifier
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
            text.Title ??= new UnityField<string> { Value = ToUnityRichTextString() };
            return text;
        }

        public override string ToString()
        {
            return $"{Descriptors[CAMType.Character].Text} {Descriptors[CAMType.Action].Text} {Descriptors[CAMType.Modifier].Text}";
        }
        public string ToHtmlColoredString()
        {
            return $"<span class='characterClass'>{Descriptors[CAMType.Character].Text}</span> <span class='actionClass'>{Descriptors[CAMType.Action].Text}</span> <span class='modifierClass'>{Descriptors[CAMType.Modifier].Text}</span>";
        }
        public string ToUnityRichTextString()
        {
            return $"<color=#2e53d2ff>{Descriptors[CAMType.Character].Text}</color> <color=#2aac89ff>{Descriptors[CAMType.Action].Text}</color> <color=#d292c8ff>{Descriptors[CAMType.Modifier].Text}</color>";
        }
    }
}
