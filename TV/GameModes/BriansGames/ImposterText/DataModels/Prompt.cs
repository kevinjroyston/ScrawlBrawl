using RoystonGame.TV.DataModels.Users;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.TV.GameModes.BriansGames.ImposterText.DataModels
{
    public class Prompt
    {
        public User Owner { get; set; }
        public string RealPrompt { get; set; }
        public string FakePrompt { get; set; }
        public ConcurrentDictionary<User, string> UsersToAnswers { get; set; } = new ConcurrentDictionary<User, string>();
        public User Imposter { get; set; }
        public ConcurrentDictionary<User, User> UsersToVotes { get; set; } = new ConcurrentDictionary<User, User>();
        public ConcurrentDictionary<User, int> UsersToNumVotesRecieved { get; set; } = new ConcurrentDictionary<User, int>();
    }
}
