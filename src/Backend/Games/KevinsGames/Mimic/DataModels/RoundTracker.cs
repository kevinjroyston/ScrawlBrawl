using Backend.GameInfrastructure.DataModels.Users;
using Backend.Games.Common.DataModels;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Backend.Games.KevinsGames.Mimic.DataModels
{
    public class RoundTracker
    {
        public ConcurrentDictionary<User, UserDrawing> UsersToUserDrawings { get; set; } = new ConcurrentDictionary<User, UserDrawing>();
        public User originalDrawer { get; set; }
        public ConcurrentDictionary<User, int> UserToNumVotesRecieved { get; set; } = new ConcurrentDictionary<User, int>();
        public List<User> UsersToDisplay { get; set; } = new List<User>();
        public ConcurrentDictionary<int, List<User>> QuestionsToUsersWhoVotedFor { get; set; } = new ConcurrentDictionary<int, List<User>>();
    }
}
