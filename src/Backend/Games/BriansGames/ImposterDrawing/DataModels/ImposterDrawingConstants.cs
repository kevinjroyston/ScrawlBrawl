﻿using Common.DataModels.Enums;
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
            { GameDuration.Short, TimeSpan.FromSeconds(90)},
            { GameDuration.Normal, TimeSpan.FromSeconds(110)},
            { GameDuration.Extended, TimeSpan.FromSeconds(120)},
        };
        public static IReadOnlyDictionary<GameDuration, TimeSpan> DrawingTimer = new Dictionary<GameDuration, TimeSpan>()
        {
            { GameDuration.Short, TimeSpan.FromSeconds(100)},
            { GameDuration.Normal, TimeSpan.FromSeconds(115)},
            { GameDuration.Extended, TimeSpan.FromSeconds(130)},
        };
        public static IReadOnlyDictionary<GameDuration, TimeSpan> VotingTimer = new Dictionary<GameDuration, TimeSpan>()
        {
            { GameDuration.Short, TimeSpan.FromSeconds(40)},
            { GameDuration.Normal, TimeSpan.FromSeconds(50)},
            { GameDuration.Extended, TimeSpan.FromSeconds(50)},
        };

        // Number of rounds (round = create contestants & vote)
        public static IReadOnlyDictionary<GameDuration, int> MaxNumRounds = new Dictionary<GameDuration, int>()
        {
            { GameDuration.Short, 5},
            { GameDuration.Normal, 8},
            { GameDuration.Extended, 14},
        };

        public const int MaxNumPlayersPerRound = 10;
        public const int MinNumPlayersPerRound = 3;
        // Number of rounds (round = create contestants & vote)
        public static IReadOnlyDictionary<GameDuration, int> MaxDrawingsPerPlayer = new Dictionary<GameDuration, int>()
        {
            { GameDuration.Short, 4},
            { GameDuration.Normal, 5},
            { GameDuration.Extended, 7},
        };
    }
}
