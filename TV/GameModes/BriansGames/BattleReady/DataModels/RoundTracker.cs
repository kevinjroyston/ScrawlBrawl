using RoystonGame.TV.DataModels;
using RoystonGame.TV.GameModes.Common.ThreePartPeople.DataModels;
using System.Collections.Generic;
using static RoystonGame.TV.GameModes.Common.ThreePartPeople.DataModels.Person;

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
