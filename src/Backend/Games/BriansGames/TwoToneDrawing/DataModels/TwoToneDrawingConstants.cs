using Common.DataModels.Enums;
using System;
using System.Collections.Generic;

namespace Backend.Games.BriansGames.TwoToneDrawing.DataModels
{
    public static class TwoToneDrawingConstants
    {
        public const int PointsPerVote = 100;
        public const int PointsForVotingForWinningDrawing = 100;


        // These timers are multiplied by 1.5f if they have to write text.
        public static IReadOnlyDictionary<GameDuration, TimeSpan> SetupTimer = new Dictionary<GameDuration, TimeSpan>()
        {
            { GameDuration.Short, TimeSpan.FromSeconds(90)},
            { GameDuration.Normal, TimeSpan.FromSeconds(120)},
            { GameDuration.Extended, TimeSpan.FromSeconds(160)},
        };
        public static IReadOnlyDictionary<GameDuration, TimeSpan> PerDrawingTimer = new Dictionary<GameDuration, TimeSpan>()
        {
            { GameDuration.Short, TimeSpan.FromSeconds(100)},
            { GameDuration.Normal, TimeSpan.FromSeconds(130)},
            { GameDuration.Extended, TimeSpan.FromSeconds(180)},
        };
        public static IReadOnlyDictionary<GameDuration, TimeSpan> VotingTimer = new Dictionary<GameDuration, TimeSpan>()
        {
            { GameDuration.Short, TimeSpan.FromSeconds(45)},
            { GameDuration.Normal, TimeSpan.FromSeconds(60)},
            { GameDuration.Extended, TimeSpan.FromSeconds(90)},
        };

        public static IReadOnlyDictionary<GameDuration, int> MaxNumRounds = new Dictionary<GameDuration, int>()
        {
            { GameDuration.Short, 5},
            { GameDuration.Normal, 6},
            { GameDuration.Extended, 9},
        };

        public static IReadOnlyDictionary<GameDuration, int> DrawingsPerPlayer = new Dictionary<GameDuration, int>()
        {
            { GameDuration.Short, 3},
            { GameDuration.Normal, 4},
            { GameDuration.Extended, 6},
        };
    }
}
