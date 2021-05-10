using System;
using System.Collections.Generic;
using System.Text;

namespace Common.DataModels.Enums
{
    /// <summary>
    /// Various target game durations. Games can tweak their timer duration as well as number of round parameter to try and reach the goals set below.
    /// </summary>
    public enum GameDuration
    {
        // A second copy of this enum exists on the frontend!!

        // All durations will try to "complete" all rounds. If there are too many users, rounds will be cut to fit under time limit.
        // As such, Normal w/ 4 players might not cut anything at all and finish in 12 minutes.
        Short, // 10 minute upper bound (ideal)
        Normal, // 20 minute upper bound (ideal)
        Extended, // 40 minute upper bound (ideal)
    }
}
