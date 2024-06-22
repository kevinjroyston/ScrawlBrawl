using Common.DataModels.Enums;
using System;
using System.Collections.Generic;

namespace Backend.Games.KevinsGames.Mimic.DataModels
{
    public static class MimicConstants
    {
        public const int PointsForVote = 100;
        public static int PointsForCorrectPick(int numChoices)
        {
            return numChoices * 100;
        }
        public const float MimicTimerMultiplier = 1.8f;

        public static readonly TimeSpan MemorizeTimerLength = TimeSpan.FromSeconds(15);

        public const double BlurDelay = 6.0;
        public const double BlurLength = 16.0;

        public static IReadOnlyDictionary<GameDuration, TimeSpan> DrawingTimer = new Dictionary<GameDuration, TimeSpan>()
        {
            { GameDuration.Short, TimeSpan.FromSeconds(45)},
            { GameDuration.Normal, TimeSpan.FromSeconds(50)},
            { GameDuration.Extended, TimeSpan.FromSeconds(60)},
        };
        public static IReadOnlyDictionary<GameDuration, TimeSpan> VotingTimer = new Dictionary<GameDuration, TimeSpan>()
        {
            { GameDuration.Short, TimeSpan.FromSeconds(40)},
            { GameDuration.Normal, TimeSpan.FromSeconds(50)},
            { GameDuration.Extended, TimeSpan.FromSeconds(60)},
        };

        // If you have more than this many players on this mode you WILL see impact.
        public static IReadOnlyDictionary<GameDuration, int> MaxRounds = new Dictionary<GameDuration, int>()
        {
            { GameDuration.Short, 5},
            { GameDuration.Normal, 7},
            { GameDuration.Extended, 9},
        };
    }
}
