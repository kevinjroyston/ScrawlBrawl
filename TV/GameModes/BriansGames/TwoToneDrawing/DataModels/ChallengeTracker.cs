using RoystonGame.TV.DataModels;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace RoystonGame.TV.GameModes.BriansGames.TwoToneDrawing.DataModels
{
    public class ChallengeTracker
    {
        /// <summary>
        /// The user who came up with this challenge
        /// </summary>
        public User Owner { get; set; }

        public string Prompt { get; set; }

        public List<string> Colors { get; set; }

        public class UserDrawing
        {
            public string Drawing { get; set; }
            public string Color { get; set; }
            public string TeamId { get; set; }
        }
        public Dictionary<User, UserDrawing> UserSubmittedDrawings { get; set; } = new Dictionary<User, UserDrawing>();

        #region Scoring / Rendering
        /// <summary>
        /// TeamId -> Color -> Drawing
        /// </summary>
        public ConcurrentDictionary<string, ConcurrentDictionary<string, string>> TeamIdToDrawingMapping { get; set; } = new ConcurrentDictionary<string, ConcurrentDictionary<string, string>>();

        public ConcurrentDictionary<string, ConcurrentBag<User>> TeamIdToUsersWhoVotedMapping { get; set; } = new ConcurrentDictionary<string, ConcurrentBag<User>>();
        #endregion
    }
}
