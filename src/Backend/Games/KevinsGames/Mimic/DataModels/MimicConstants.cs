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
        public const float MimicTimerMultiplier = 2.0f;

        public static readonly TimeSpan MemorizeTimerLength = TimeSpan.FromSeconds(10);

        public const double BlurDelay = 5.0;
        public const double BlurLength = 15.0;

        public static IReadOnlyDictionary<GameDuration, TimeSpan> DrawingTimer = new Dictionary<GameDuration, TimeSpan>()
        {
            { GameDuration.Short, TimeSpan.FromSeconds(30)},
            { GameDuration.Normal, TimeSpan.FromSeconds(45)},
            { GameDuration.Extended, TimeSpan.FromSeconds(60)},
        };
        public static IReadOnlyDictionary<GameDuration, TimeSpan> VotingTimer = new Dictionary<GameDuration, TimeSpan>()
        {
            { GameDuration.Short, TimeSpan.FromSeconds(30)},
            { GameDuration.Normal, TimeSpan.FromSeconds(60)},
            { GameDuration.Extended, TimeSpan.FromSeconds(90)},
        };

        // If you have more than this many players on this mode you WILL see impact.
        public static IReadOnlyDictionary<GameDuration, int> MaxRounds = new Dictionary<GameDuration, int>()
        {
            { GameDuration.Short, 6},
            { GameDuration.Normal, 12},
            { GameDuration.Extended, 20},
        };
    }
}
