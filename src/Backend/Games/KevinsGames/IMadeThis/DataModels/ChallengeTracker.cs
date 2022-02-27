using Backend.GameInfrastructure.DataModels.Users;
using Backend.Games.Common.DataModels;
using Common.DataModels.Interfaces;
using System;
using System.Collections.Concurrent;

namespace Backend.Games.KevinsGames.IMadeThis.DataModels
{
    public class ChallengeTracker : Constraints<User>
    {
        public string InitialPrompt { get; set; }
        public UserDrawing InitialDrawing { get; set; }
        public string SecondaryPrompt { get; set; }
        public ConcurrentDictionary<User, UserDrawing> UsersToDrawings { get; set; } = new ConcurrentDictionary<User, UserDrawing>();

        public override bool? AllowDuplicateIds { get; set; } = false;
    }
}
