using Backend.GameInfrastructure.DataModels.Users;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Backend.Games.KevinsGames.StoryTime.DataModels
{
    public class RoundTracker
    {
        public class UserWriting
        {
            public User Owner { get; set; }
            public string Text { get; set; }
            public WritingDisplayPosition Position { get; set; }
            public int VotesRecieved { get; set; } = 0;
            public UserWriting(User owner, string text, WritingDisplayPosition position)
            {
                Owner = owner;
                Text = text;
                Position = position;
            }
        }
        public enum WritingDisplayPosition
        {
            Before,
            After,
            None
        };
        public ConcurrentDictionary<User, UserWriting> UsersToUserWriting {get;set;} = new ConcurrentDictionary<User, UserWriting>();
        public UserWriting Winner { get; set; }
        public List<User> UsersToDisplay { get; internal set; }
    }
}
