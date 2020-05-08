using RoystonGame.TV.DataModels;
using RoystonGame.TV.GameModes.Common.ThreePartPeople.DataModels;
using System.Collections.Generic;
using static RoystonGame.TV.GameModes.Common.ThreePartPeople.DataModels.Person;

namespace RoystonGame.TV.GameModes.BriansGames.BattleReady.DataModels
{
    public class RoundTracker
    {
        public Dictionary<User, List<PeopleUserDrawing>> UsersToAssignedBodyParts { get; set; } = new Dictionary<User, List<PeopleUserDrawing>>();
        public Dictionary<User, string> UsersToAssignedPrompts { get; set; } = new Dictionary<User, string>();
        public Dictionary<User, Person> UsersToBuiltPerson { get; set; } = new Dictionary<User, Person>();
        public List<User> OrderedUsers { get; set; } = new List<User>();

        public void ResetRoundVariables()
        {
            UsersToAssignedBodyParts = new Dictionary<User, List<PeopleUserDrawing>>();
            UsersToAssignedPrompts = new Dictionary<User, string>();
            UsersToBuiltPerson = new Dictionary<User, Person>();
            OrderedUsers = new List<User>();
        }
    }
}
