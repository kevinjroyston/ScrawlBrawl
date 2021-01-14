using Backend.GameInfrastructure.DataModels.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Games.BriansGames.HintHint.DataModels
{
    public class RealFakePair
    {
        public string RealGoal { get; set; }
        public string FakeGoal { get; set; }
        public List<string> BannedWords { get; set; } 
        public List<User> RealHintGivers { get; set; }
        public List<User> FakeHintGivers { get; set; }
    }
}
