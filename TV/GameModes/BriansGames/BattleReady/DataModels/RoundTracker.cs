using RoystonGame.TV.DataModels.Users;
using System.Collections.Generic;

namespace RoystonGame.TV.GameModes.BriansGames.BattleReady.DataModels
{
    public class RoundTracker
    {
        public Dictionary<User, List<Prompt>> UsersToAssignedPrompts { get; set; } = new Dictionary<User, List<Prompt>>();
        
        public List<Prompt> RoundPrompts { get; set; } = new List<Prompt>();
        public void ResetRoundVariables()
        {
            UsersToAssignedPrompts = new Dictionary<User, List<Prompt>>();
            RoundPrompts = new List<Prompt>();
        }
    }
}
