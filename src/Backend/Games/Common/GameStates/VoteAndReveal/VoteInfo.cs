using Backend.GameInfrastructure.DataModels.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Games.Common.GameStates.VoteAndReveal
{
    public class VoteInfo
    {
        public User UserWhoVoted { get; set; }
        public double TimeTakenInMs { get; set; }
        public List<object> ObjectsVotedFor { get; set; }
    }
}
