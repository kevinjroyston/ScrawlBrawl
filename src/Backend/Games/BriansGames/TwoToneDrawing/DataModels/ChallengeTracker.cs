using Backend.GameInfrastructure.DataModels.Users;
using Backend.Games.Common.DataModels;
using Common.DataModels.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Backend.Games.BriansGames.TwoToneDrawing.DataModels
{
    public class ChallengeTracker
        : Constraints<User>
    {
        /// <summary>
        /// The user who came up with this challenge
        /// </summary>
        public User Owner { get; set; }

        public string Prompt { get; set; }

        public List<string> Colors { get; set; }

        public class TeamUserDrawing: UserDrawing
        {
            public string Color { get; set; }
            public string TeamId { get; set; }
        }
        public Dictionary<User, TeamUserDrawing> UserSubmittedDrawings { get; set; } = new Dictionary<User, TeamUserDrawing>();

        #region Scoring / Rendering
        /// <summary>
        /// TeamId -> Color -> Drawing
        /// </summary>
        public ConcurrentDictionary<string, ConcurrentDictionary<string, TeamUserDrawing>> TeamIdToDrawingMapping { get; set; } = new ConcurrentDictionary<string, ConcurrentDictionary<string, TeamUserDrawing>>();

        public ConcurrentDictionary<string, ConcurrentBag<User>> TeamIdToUsersWhoVotedMapping { get; set; } = new ConcurrentDictionary<string, ConcurrentBag<User>>();

        #endregion

        public override bool? AllowDuplicateIds => false;
    }
}
