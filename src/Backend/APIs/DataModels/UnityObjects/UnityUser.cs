using Backend.GameInfrastructure.DataModels.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.APIs.DataModels.UnityObjects
{
    public class UnityUser
    {
        public Guid Id => User.Id;
        public string DisplayName => User.DisplayName;
        public string SelfPortrait => User.SelfPortrait;
        public int Score => User.Score;
        public int ScoreDeltaReveal => User.ScoreDeltaReveal;
        public int ScoreDeltaScoreboard => User.ScoreDeltaScoreboard;
        public UserActivity Activity => User.Activity;
        public UserStatus Status => User.Status;

        private User User { get; set; }

        public UnityUser(User user)
        {
            this.User = user;
        }
    }
}
