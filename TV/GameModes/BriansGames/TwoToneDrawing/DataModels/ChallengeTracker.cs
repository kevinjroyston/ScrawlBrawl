using RoystonGame.TV.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;

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
        public Dictionary<string, Dictionary<string, string>> TeamIdToDrawingMapping { get; set; } = new Dictionary<string, Dictionary<string, string>>();

        public Dictionary<string, List<User>> TeamIdToUsersWhoVotedMapping { get; set; } = new Dictionary<string, List<User>>();
        #endregion
    }
}
