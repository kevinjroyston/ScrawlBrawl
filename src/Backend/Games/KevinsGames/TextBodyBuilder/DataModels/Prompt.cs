using Backend.APIs.DataModels.UnityObjects;
using Backend.GameInfrastructure.DataModels.Users;
using Backend.Games.Common.DataModels;
using Backend.Games.Common.ThreePartPeople.DataModels;
using Common.DataModels.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using static Backend.Games.Common.ThreePartPeople.DataModels.Person;
using static Backend.Games.KevinsGames.TextBodyBuilder.DataModels.TextPerson;

namespace Backend.Games.KevinsGames.TextBodyBuilder.DataModels
{
    public class Prompt : UserCreatedObject, IConstraints<User>
    {
        public string Text { get; set; }
        public User Winner { get; set; }
        public class UserHand : TextPerson
        {
            public List<CAMUserText> CharacterChoices { get; set; } = new List<CAMUserText>();
            public List<CAMUserText> ActionChoices { get; set; } = new List<CAMUserText>();
            public List<CAMUserText> ModifierChoices { get; set; } = new List<CAMUserText>();
        }
        public ConcurrentDictionary<User, UserHand> UsersToUserHands { get; set; } = new ConcurrentDictionary<User, UserHand>();

        #region Assignment Constraints
        public ImmutableHashSet<Guid> BannedMemberIds { get; } = null;

        public int? MaxMemberCount { get; set; } = null;

        public ImmutableHashSet<Guid> BannedMemberTags { get; } = null;

        public ImmutableHashSet<Guid> AllowedMemberTags { get; } = null;

        public bool? AllowDuplicateIds { get; } = false;

        public bool? AllowDuplicateTags { get; } = null;

        public bool? AllowDuplicateFromSource { get; } = null;
        #endregion
    }
}
