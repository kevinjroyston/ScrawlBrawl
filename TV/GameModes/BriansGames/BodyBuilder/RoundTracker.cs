using RoystonGame.TV.DataModels;
using RoystonGame.TV.GameModes.BriansGames.BodyBuilder.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.TV.GameModes.BriansGames.BodyBuilder
{
    public static class RoundTracker
    {
        public static List<Gameplay_Person> UnassignedPeople { get; set; } = new List<Gameplay_Person>();
        public static Dictionary<User, Gameplay_Person> AssignedPeople { get; set; } = new Dictionary<User, Gameplay_Person>();
        public static Dictionary<User, int> UsersToSeatNumber { get; set; } = new Dictionary<User, int>();
        public static List<User> OrderedUsers { get; set; } = new List<User>();

        public static void ResetRoundVariables()
        {
            UnassignedPeople = new List<Gameplay_Person>();
            AssignedPeople = new Dictionary<User, Gameplay_Person>();
            UsersToSeatNumber = new Dictionary<User, int>();
            OrderedUsers = new List<User>();
        }
    }
}
