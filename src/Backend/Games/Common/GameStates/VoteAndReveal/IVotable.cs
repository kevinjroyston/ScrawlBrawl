﻿using Backend.APIs.DataModels.UnityObjects;
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
        public List<VoteInfo> VotesCastForThisObject { get; }
        public abstract UnityImage VotingUnityObjectGenerator(int numericId);
        public abstract UnityImage RevealUnityObjectGenerator(int numericId);
        public bool ShouldHighlightReveal { get; }
    }
}