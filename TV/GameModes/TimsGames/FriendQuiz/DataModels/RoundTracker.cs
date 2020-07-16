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
        public ConcurrentBag<Question> Questions { get; set; } = new ConcurrentBag<Question>();
    }
}
