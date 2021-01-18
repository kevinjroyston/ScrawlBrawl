using Backend.APIs.DataModels.UnityObjects;
using Backend.GameInfrastructure.DataModels.Users;
using Common.DataModels.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Games.BriansGames.HintHint.DataModels
{
    public enum EndCondition
    {
        GuessedReal,
        GuessedFake,
        Timeout
    }
    public class RealFakePair : Constraints<User>
    {
        private Random Rand = new Random();
        public string RealGoal { get; set; }
        public string FakeGoal { get; set; }
        public List<string> BannedWords { get; set; } 
        public List<User> RealHintGivers { get; set; }
        public List<User> FakeHintGivers { get; set; }
        public EndCondition RoundEndCondition { get; set; }
        public List<StackItemHolder> LastHints { get; set; }
        public List<User> RelaventUsersWhoGuessed { get; set; }

        public IReadOnlyList<Guid> Tags => new List<Guid>();
        public ImmutableHashSet<Guid> BannedMemberIds = ImmutableHashSet.Create<Guid>(); 

        public User Creator { get; set; }

        public void PopulateHintGivers(List<User> users, int numFakegivers)
        {
            if (!users.Contains(Creator))
            {
                //should never not be true but just in case
                users.Add(Creator);
            }
            users = users.OrderBy(_ => Rand.Next()).ToList();
            this.FakeHintGivers = users.GetRange(0, numFakegivers);
            users.RemoveRange(0, numFakegivers);
            this.RealHintGivers = users;
        }

    }
}
