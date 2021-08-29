using Backend.GameInfrastructure.DataModels.Users;
using System.Collections.Generic;

namespace Backend.Games.KevinsGames.TextBodyBuilder.DataModels
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
