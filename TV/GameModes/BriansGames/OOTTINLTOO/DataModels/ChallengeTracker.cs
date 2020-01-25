using RoystonGame.TV.DataModels;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace RoystonGame.TV.GameModes.BriansGames.OOTTINLTOO.DataModels
{
    public class ChallengeTracker
    {
        /// <summary>
        /// The user who came up with this challenge
        /// </summary>
        public User Owner { get; set; }

        public string RealPrompt { get; set; }
        public string DeceptionPrompt { get; set; }

        public Dictionary<User, string> UserSubmittedDrawings { get; set; } = new Dictionary<User, string>();
        public User OddOneOut { get; set; }

        #region Scoring / Rendering
        /// <summary>
        /// Gets populated by gameplay and used for scoring and rendering the drawings. Contains a randomized ordering of drawings.
        /// </summary>
        public ConcurrentDictionary<string, (User, string)> IdToDrawingMapping { get; set; } = new ConcurrentDictionary<string, (User, string)>();
        public ConcurrentBag<User> UsersWhoFoundOOO { get; set; } = new ConcurrentBag<User>();
        public ConcurrentDictionary<User, ConcurrentBag<User>> UsersWhoConfusedWhichUsers { get; set; } = new ConcurrentDictionary<User, ConcurrentBag<User>>();
        #endregion
    }
}
