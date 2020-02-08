using RoystonGame.TV.DataModels;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace RoystonGame.TV.GameModes.KylesGames.QuestionQuest.DataModels
{
    public class ChallengeTracker
    {
        /// <summary>
        /// The user who came up with the question for this challenge.
        /// </summary>
        public User DrawingOwner { get; set; }
        public string Drawing { get; set; }
        public List<string> Answers { get; set; }
        public ConcurrentDictionary<int, ConcurrentBag<User>> AnswersToUsersWhoSelected { get; set; } = new ConcurrentDictionary<int, ConcurrentBag<User>>();

        public int AchillesAnswer { get; set; }
    }
}
