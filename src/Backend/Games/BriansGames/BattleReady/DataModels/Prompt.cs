using Backend.GameInfrastructure.DataModels.Users;
using Backend.Games.Common.DataModels;
using Backend.Games.Common.ThreePartPeople.DataModels;
using Common.DataModels.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using static Backend.Games.Common.ThreePartPeople.DataModels.Person;

namespace Backend.Games.BriansGames.BattleReady.DataModels
{
    public class Prompt : UserCreatedObject, IConstraints<User>
    {
        public string Text { get; set; }
        public User Winner { get; set; }
        public class UserHand
        {
            public List<PeopleUserDrawing> Heads { get; set; } = new List<PeopleUserDrawing>();
            public List<PeopleUserDrawing> Bodies { get; set; } = new List<PeopleUserDrawing>();
            public List<PeopleUserDrawing> Legs { get; set; } = new List<PeopleUserDrawing>();
            public Person Contestant { get; set; } = new Person();
            public int VotesForContestant { get; set; } = 0;
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
