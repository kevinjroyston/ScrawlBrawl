using Common.DataModels.Enums;
using System;
using System.Collections.Generic;

namespace Backend.Games.KevinsGames.IMadeThis.DataModels
{
    public static class IMadeThisConstants
    {
        public const int PointsPerVote = 100;
        public const int PointsForVotingForWinningDrawing = 150;

        public static IReadOnlyDictionary<GameDuration, TimeSpan> WritingTimer = new Dictionary<GameDuration, TimeSpan>()
        {
            { GameDuration.Short, TimeSpan.FromSeconds(90)},
            { GameDuration.Normal, TimeSpan.FromSeconds(110)},
            { GameDuration.Extended, TimeSpan.FromSeconds(160)},
        };
        public static IReadOnlyDictionary<GameDuration, TimeSpan> DrawingTimer = new Dictionary<GameDuration, TimeSpan>()
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

        // Number of rounds (a round is one original drawing and new prompt for it, with many players submitting)
        public static IReadOnlyDictionary<GameDuration, int> MaxNumRounds = new Dictionary<GameDuration, int>()
        {
            { GameDuration.Short, 5},
            { GameDuration.Normal, 7},
            { GameDuration.Extended, 9},
        };

        public const int MaxNumPlayersPerRound = 5;
        public const int MinNumPlayersPerRound = 2;

        public static IReadOnlyDictionary<GameDuration, int> MaxDrawingsPerPlayer = new Dictionary<GameDuration, int>()
        {
            { GameDuration.Short, 3},
            { GameDuration.Normal, 4},
            { GameDuration.Extended, 5},
        };
    }
}
