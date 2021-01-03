using Backend.APIs.DataModels.UnityObjects;
using Backend.GameInfrastructure.DataModels.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Games.Common.GameStates.VoteAndReveal
{
    /// <summary>
    /// Must be implemented in order to be voted on.
    /// </summary>
    public interface IVotable
    {
        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public List<VoteInfo> VotesCastForThisObject { get; }
        
        public abstract Legacy_UnityImage VotingUnityObjectGenerator(int numericId);
        public abstract Legacy_UnityImage RevealUnityObjectGenerator(int numericId);
        public bool ShouldHighlightReveal { get; }
    }
}
