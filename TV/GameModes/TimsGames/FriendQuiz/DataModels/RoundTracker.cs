using RoystonGame.TV.DataModels.Users;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.TV.GameModes.TimsGames.FriendQuiz.DataModels
{
    public class RoundTracker
    {
        public ConcurrentDictionary<User, Question> UsersToQuestions { get; set; } = new ConcurrentDictionary<User, Question>();
    }
}
