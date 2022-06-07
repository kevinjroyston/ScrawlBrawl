using Common.DataModels.Enums;
using System;
using System.Collections.Generic;

namespace Backend.Games.KevinsGames.LateToArtClass.DataModels
{
    public static class LateToArtClassConstants
    {
        // Freebie - Lost*NumVotes will be clipped to be >= 0
        public const int FreebiePointsForNormal = 150;
        public const int LostPointsForBadNormal = 50;

        public const int PointsForCorrectAnswer = 500;

        //Found who they copied from, not the imposter
        public const int PointsForPartialCorrectAnswer = 250;
        public const int BonusPointsForGoodImposter = 600;

        public static IReadOnlyDictionary<GameDuration, TimeSpan> WritingTimer = new Dictionary<GameDuration, TimeSpan>()
        {
            { GameDuration.Short, TimeSpan.FromSeconds(90)},
            { GameDuration.Normal, TimeSpan.FromSeconds(120)},
            { GameDuration.Extended, TimeSpan.FromSeconds(180)},
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

        // Number of art classes (one prompt, many drawings, one late student)
        public static IReadOnlyDictionary<GameDuration, int> MaxNumRounds = new Dictionary<GameDuration, int>()
        {
            { GameDuration.Short, 5},
            { GameDuration.Normal, 7},
            { GameDuration.Extended, 9},
        };

        public const int MaxNumPlayersPerRound = 8;
        public const int MinNumPlayersPerRound = 3;

        public static IReadOnlyDictionary<GameDuration, int> MaxDrawingsPerPlayer = new Dictionary<GameDuration, int>()
        {
            { GameDuration.Short, 4},
            { GameDuration.Normal, 5},
            { GameDuration.Extended, 6},
        };
    }
}
