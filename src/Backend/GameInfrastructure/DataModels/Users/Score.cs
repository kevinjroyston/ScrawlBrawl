using Backend.APIs.DataModels.UnityObjects;
using Common.Code.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.GameInfrastructure.DataModels.Users
{
    public class Score : IAccessorHashable
    {
        public IReadOnlyDictionary<Scope, int> ScoreAggregates => _ScoreAggregates;
        public IReadOnlyDictionary<Scope, IReadOnlyDictionary<Reason, int>> ScoreBreakdowns => _ScoreBreakdowns.ToDictionary((kvp)=>kvp.Key, (kvp)=>(IReadOnlyDictionary<Reason,int>)kvp.Value);

        private ConcurrentDictionary<Scope, int> _ScoreAggregates { get; set; }

        private ConcurrentDictionary<Scope, ConcurrentDictionary<Reason, int>> _ScoreBreakdowns { get; set; } = new ConcurrentDictionary<Scope, ConcurrentDictionary<Reason, int>>();

        public enum Scope
        {
            Reveal,
            Scoreboard,
            Total
        }
        public enum Reason
        {
            Other = 0,
            CorrectAnswer,
            ReceivedVotes,
            CorrectAnswerSpeed,
            DrawingUsed,
            Finished,
            VotedWithCrowd,
            Imposter_GoodNormal,
            Imposter_GoodImposter,
        }
        public IReadOnlyDictionary<Reason, string> ReasonDescriptions { get; } = new Dictionary<Reason, string>
        {
            { Reason.Other, "Other" },
            { Reason.CorrectAnswer, "Correct Answer" },
            { Reason.Imposter_GoodNormal, "Blended in when not imposter" },
            { Reason.Imposter_GoodImposter, "Standing out as imposter" },
            { Reason.ReceivedVotes, "Received Votes" },
            { Reason.CorrectAnswerSpeed, "Correct Answer (Speed)" },
            { Reason.DrawingUsed, "Drawing Used" },
            { Reason.Finished, "Finished (Speed)" },
            { Reason.VotedWithCrowd, "In agreement with other voters" },
        };
        public void ResetScore(Scope? scope = null)
        {
            // If no scope is specified, reset everything.
            if (!scope.HasValue)
            {
                foreach (Scope scp in Enum.GetValues(typeof(Scope)))
                {
                    ResetScore(scp);
                }
            }

            _ScoreAggregates[scope.Value] = 0;
            _ScoreBreakdowns[scope.Value] = new ConcurrentDictionary<Reason, int>();
        }
        public void ResetScoreBreakdownsOnly(Scope? scope = null)
        {
            // If no scope is specified, reset everything.
            if (!scope.HasValue)
            {
                foreach (Scope scp in Enum.GetValues(typeof(Scope)))
                {
                    ResetScoreBreakdownsOnly(scp);
                }
            }

            _ScoreBreakdowns[scope.Value] = new ConcurrentDictionary<Reason, int>();
        }

        public void AddScore(int amount, Reason reason)
        {
            foreach (Scope scp in Enum.GetValues(typeof(Scope)))
            {
                _ScoreAggregates.AddOrIncrease(scp, amount, amount);
                _ScoreBreakdowns.GetOrAdd(scp, new ConcurrentDictionary<Reason, int>()).AddOrIncrease(reason, amount, amount);
            }
        }

        public int GetIAccessorHashCode()
        {
            var hash = new HashCode();
            foreach (Scope scp in Enum.GetValues(typeof(Scope)))
            {
                hash.Add(_ScoreAggregates.GetOrAdd(scp, 0));
            }
            return hash.ToHashCode();
        }
    }
}
