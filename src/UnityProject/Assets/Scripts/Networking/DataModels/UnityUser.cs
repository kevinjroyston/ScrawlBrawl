using Assets.Scripts.Networking.DataModels.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Networking.DataModels
{
    public enum UserActivity
    {
        Active,
        Inactive
    }
    public enum UserStatus
    {
        AnsweringPrompts,
        Waiting,
    }

    public class UnityUser : OptionsInterface<UnityUserOptions>
    {
        public Guid Id { get; set; }
        public string DisplayName { get; set; }
        public string SelfPortait { get; set; }
        public int Score { get; set; }
        public int ScoreDeltaReveal { get; set; }
        public int ScoreDeltaScoreboard { get; set; }
        public UserActivity Activity {get; set;}
        public UserStatus Status { get; set; }

        public Dictionary<UnityUserOptions, object> Options { get; set; }

    }
}
