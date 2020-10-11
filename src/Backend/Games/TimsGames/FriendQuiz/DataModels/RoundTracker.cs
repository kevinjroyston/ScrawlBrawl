using System.Collections.Concurrent;

namespace Backend.Games.TimsGames.FriendQuiz.DataModels
{
    public class RoundTracker
    {
        public ConcurrentBag<Question> Questions { get; set; } = new ConcurrentBag<Question>();
    }
}
