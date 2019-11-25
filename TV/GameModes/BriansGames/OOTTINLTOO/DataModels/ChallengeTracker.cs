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
    }
}
