using Common.DataModels.Enums;
using System;
using System.Collections.Generic;

namespace Backend.Games.BriansGames.BattleReady.DataModels
{
    public static class BattleReadyConstants
    {
        public const int PointsForVote = 100;
        public const int PointsForVotingForWinningDrawing = 150;
        public const int PointsForPartUsed = 50;
        public const int NumDrawingsInHand = 3;

        public static IReadOnlyDictionary<GameDuration, TimeSpan> SetupPerDrawingTimer = new Dictionary<GameDuration, TimeSpan>()
        {
            { GameDuration.Short, TimeSpan.FromSeconds(100)},
            { GameDuration.Normal, TimeSpan.FromSeconds(130)},
            { GameDuration.Extended, TimeSpan.FromSeconds(180)},
        };
        public static IReadOnlyDictionary<GameDuration, TimeSpan> SetupPerPromptTimer = new Dictionary<GameDuration, TimeSpan>()
        {
            { GameDuration.Short, TimeSpan.FromSeconds(75)},
            { GameDuration.Normal, TimeSpan.FromSeconds(90)},
            { GameDuration.Extended, TimeSpan.FromSeconds(110)},
        };
        public static IReadOnlyDictionary<GameDuration, TimeSpan> PerCreationTimer = new Dictionary<GameDuration, TimeSpan>()
        {
            { GameDuration.Short, TimeSpan.FromSeconds(50)},
            { GameDuration.Normal, TimeSpan.FromSeconds(60)},
            { GameDuration.Extended, TimeSpan.FromSeconds(80)},
        };
        public static IReadOnlyDictionary<GameDuration, TimeSpan> VotingTimer = new Dictionary<GameDuration, TimeSpan>()
        {
            { GameDuration.Short, TimeSpan.FromSeconds(40)},
            { GameDuration.Normal, TimeSpan.FromSeconds(50)},
            { GameDuration.Extended, TimeSpan.FromSeconds(60)},
        };

        // Number of rounds (round = create contestants & vote)
        public static IReadOnlyDictionary<GameDuration, int> NumRounds = new Dictionary<GameDuration, int>()
        {
            { GameDuration.Short, 1},
            { GameDuration.Normal, 2},
            { GameDuration.Extended, 2},
        };

        // SubRound = 1 prompt. By default 1 per player.
        public static IReadOnlyDictionary<GameDuration, int> MaxNumSubRounds = new Dictionary<GameDuration, int>()
        {
            { GameDuration.Short, 4},
            { GameDuration.Normal, 4},
            { GameDuration.Extended, 6},
        };

        public static IReadOnlyDictionary<GameDuration, int> NumDrawingsPerPlayer = new Dictionary<GameDuration, int>()
        {
            { GameDuration.Short, 3},
            { GameDuration.Normal, 4},
            { GameDuration.Extended, 5},
        };
    }
}
