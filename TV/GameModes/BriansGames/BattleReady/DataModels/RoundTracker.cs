using RoystonGame.TV.DataModels;
using RoystonGame.TV.GameModes.Common.ThreePartPeople.DataModels;
using System.Collections.Generic;
using static RoystonGame.TV.GameModes.Common.ThreePartPeople.DataModels.Person;

namespace RoystonGame.TV.GameModes.BriansGames.BattleReady.DataModels
{
    public class RoundTracker
    {
        /********************
         * Game Consists of rounds made up of subrounds
         * the number of subrounds is equivalent to the number of prompts the player builds a character for each round
         ******************/
        public List<Dictionary<User, List<PeopleUserDrawing>>> UsersToPlayerHandsHeadsBySubRound { get; set; } = new List<Dictionary<User, List<PeopleUserDrawing>>>();
        public List<Dictionary<User, List<PeopleUserDrawing>>> UsersToPlayerHandsBodiesBySubRound { get; set; } = new List<Dictionary<User, List<PeopleUserDrawing>>>();
        public List<Dictionary<User, List<PeopleUserDrawing>>> UsersToPlayerHandsLegsBySubRound { get; set; } = new List<Dictionary<User, List<PeopleUserDrawing>>>();
        public Dictionary<User, List<string>> UsersToAssignedPrompts { get; set; } = new Dictionary<User, List<string>>();
        public Dictionary<string, List<Person>> PromptsToBuiltPeople { get; set; } = new Dictionary<string, List<Person>>();
        public Dictionary<User, List<Person>> UsersToBuiltPeople { get; set; } = new Dictionary<User, List<Person>>();
        public Dictionary<Person, string> BuiltPeopleToPrompts { get; set; } = new Dictionary<Person, string>();
        public List<User> OrderedUsers { get; set; } = new List<User>();
        public List<string> UnusedUserPrompts { get; set; } = new List<string>(); // not reset each round
        public int pointsForVote { get; set; } = 100;

        public void ResetRoundVariables()
        {
            UsersToPlayerHandsHeadsBySubRound = new List<Dictionary<User, List<PeopleUserDrawing>>>();
            UsersToPlayerHandsBodiesBySubRound = new List<Dictionary<User, List<PeopleUserDrawing>>>();
            UsersToPlayerHandsLegsBySubRound = new List<Dictionary<User, List<PeopleUserDrawing>>>();
            BuiltPeopleToPrompts = new Dictionary<Person, string>();
            PromptsToBuiltPeople = new Dictionary<string, List<Person>>();
            UsersToAssignedPrompts = new Dictionary<User, List<string>>();
            UsersToBuiltPeople = new Dictionary<User, List<Person>>();
            OrderedUsers = new List<User>();
        }
    }
}
