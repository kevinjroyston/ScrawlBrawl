using Backend.GameInfrastructure.DataModels.Users;
using Backend.Games.Common.DataModels;
using Common.DataModels.Interfaces;
using System.Collections.Concurrent;

namespace Backend.Games.BriansGames.ImposterDrawing.DataModels
{
    public class Prompt : Constraints<User>
    {
        public User Owner { get; set; }
        public string RealPrompt { get; set; }
        public string FakePrompt { get; set; }
        public ConcurrentDictionary<User, UserDrawing> UsersToDrawings { get; set; } = new ConcurrentDictionary<User, UserDrawing>();
        public User Imposter { get; set; }
        public ConcurrentDictionary<User, User> UsersToVotes { get; set; } = new ConcurrentDictionary<User, User>();
        public ConcurrentDictionary<User, int> UsersToNumVotesRecieved { get; set; } = new ConcurrentDictionary<User, int>();
        public object UsersToAnswers { get; internal set; }
    }
}
