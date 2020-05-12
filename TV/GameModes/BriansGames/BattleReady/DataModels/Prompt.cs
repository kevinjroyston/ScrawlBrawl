using RoystonGame.TV.DataModels;
using RoystonGame.TV.GameModes.Common.ThreePartPeople.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static RoystonGame.TV.GameModes.Common.ThreePartPeople.DataModels.Person;

namespace RoystonGame.TV.GameModes.BriansGames.BattleReady.DataModels
{
    public class Prompt
    {
        public User Owner { get; set; }
        public string Text { get; set; }
        public Dictionary<User, Person> UsersToCreatedPerson { get; set; }

        public class UserHand
        {
            public List<PeopleUserDrawing> Heads { get; set; } = new List<PeopleUserDrawing>();
            public List<PeopleUserDrawing> Bodies { get; set; } = new List<PeopleUserDrawing>();
            public List<PeopleUserDrawing> Legs { get; set; } = new List<PeopleUserDrawing>();
            public Person Contestant { get; set; } = new Person();
        }
        public Dictionary<User, UserHand> UsersToUserHands { get; set; } = new Dictionary<User, UserHand>();
    }
}
