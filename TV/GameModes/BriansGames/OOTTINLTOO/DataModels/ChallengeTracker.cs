using RoystonGame.TV.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        public Dictionary<string, (User, string)> IdToDrawingMapping { get; set; } = new Dictionary<string, (User, string)>();
        public List<User> UsersWhoFoundOOO { get; set; } = new List<User>();
        public Dictionary<User, List<User>> UsersWhoConfusedWhichUsers { get; set; } = new Dictionary<User, List<User>>();
        #endregion
    }
}
