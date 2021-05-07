using Common.DataModels.Enums;
using System;
using System.Collections.Generic;

namespace Backend.Games.BriansGames.ImposterDrawing.DataModels
{
    public static class ImposterDrawingConstants
    {
        // Freebie - Lost*NumVotes will be clipped to be >= 0
        public const int FreebiePointsForNormal = 150;
        public const int LostPointsForBadNormal = 50;

        public const int PointsForCorrectAnswer = 500;
        public const int BonusPointsForGoodImposter = 300;

        public static IReadOnlyDictionary<GameDuration, TimeSpan> WritingTimer = new Dictionary<GameDuration, TimeSpan>()
        {
            { GameDuration.Short, TimeSpan.FromSeconds(45)},
            { GameDuration.Normal, TimeSpan.FromSeconds(120)},
            { GameDuration.Extended, TimeSpan.FromSeconds(180)},
        };
        public static IReadOnlyDictionary<GameDuration, TimeSpan> DrawingTimer = new Dictionary<GameDuration, TimeSpan>()
        {
            { GameDuration.Short, TimeSpan.FromSeconds(45)},
            { GameDuration.Normal, TimeSpan.FromSeconds(90)},
            { GameDuration.Extended, TimeSpan.FromSeconds(120)},
        };
        public static IReadOnlyDictionary<GameDuration, TimeSpan> VotingTimer = new Dictionary<GameDuration, TimeSpan>()
        {
            { GameDuration.Short, TimeSpan.FromSeconds(30)},
            { GameDuration.Normal, TimeSpan.FromSeconds(60)},
            { GameDuration.Extended, TimeSpan.FromSeconds(90)},
        };

        // Number of rounds (round = create contestants & vote)
        public static IReadOnlyDictionary<GameDuration, int> MaxNumRounds = new Dictionary<GameDuration, int>()
        {
            { GameDuration.Short, 6},
            { GameDuration.Normal, 8},
            { GameDuration.Extended, 12},
        };

        public const int MaxNumPlayersPerRound = 10;
        // Number of rounds (round = create contestants & vote)
        public static IReadOnlyDictionary<GameDuration, int> MaxDrawingsPerPlayer = new Dictionary<GameDuration, int>()
        {
            { GameDuration.Short, 3},
            { GameDuration.Normal, 4},
            { GameDuration.Extended, 6},
        };
    }
}
