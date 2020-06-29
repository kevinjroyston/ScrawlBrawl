using RoystonGame.TV.DataModels.Users;
using RoystonGame.TV.GameModes.Common.DataModels;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.TV.GameModes.KevinsGames.Mimic.DataModels
{
    public class RoundTracker
    {
        public ConcurrentDictionary<User, UserDrawing> UsersToUserDrawings { get; set; } = new ConcurrentDictionary<User, UserDrawing>();
        public User originalDrawer { get; set; }
        public ConcurrentDictionary<User, int> UserToNumVotesRecieved { get; set; } = new ConcurrentDictionary<User, int>();
        public List<User> UsersToDisplay { get; set; } = new List<User>();
    }
}
