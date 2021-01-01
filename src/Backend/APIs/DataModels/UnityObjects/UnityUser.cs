using Backend.GameInfrastructure.DataModels.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.APIs.DataModels.UnityObjects
{
    public class UnityUser
    {
        public Guid Id { get; set; }
        public string DisplayName { get; set; }
        public string SelfPortrait { get; set; }
        public int Score { get; set; }
        public int ScoreDeltaReveal { get; set; }
        public int ScoreDeltaScoreboard { get; set; }
        public UserActivity Activity { get; set; }
        public UserStatus Status { get; set; }

        public UnityUser(User user)
        {
            this.Id = user.Id;
            this.DisplayName = user.DisplayName;
            this.SelfPortrait = user.SelfPortrait;
            this.Score = user.Score;
            this.ScoreDeltaReveal = user.ScoreDeltaReveal;
            this.ScoreDeltaScoreboard = user.ScoreDeltaScoreboard;
            this.Activity = user.Activity;
            this.Status = user.Status;
        }
    }
}
