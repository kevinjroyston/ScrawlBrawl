using Common.DataModels.Enums;
using System;
using System.Collections.Generic;

namespace Backend.Games.KevinsGames.TextBodyBuilder.DataModels
{
    public static class TextBodyBuilderConstants
    {
        public const int PointsForVote = 100;
        public const int MaxPointsForVotingWithCrowd = 150;
        public const int PointsForPartUsed = 50;
        public const int NumCAMsInHand = 3;

        public static IReadOnlyDictionary<GameDuration, TimeSpan> SetupPerCAMTimer = new Dictionary<GameDuration, TimeSpan>()
        {
            { GameDuration.Short, TimeSpan.FromSeconds(60)},
            { GameDuration.Normal, TimeSpan.FromSeconds(70)},
            { GameDuration.Extended, TimeSpan.FromSeconds(80)},
        };
        public static IReadOnlyDictionary<GameDuration, TimeSpan> SetupPerPromptTimer = new Dictionary<GameDuration, TimeSpan>()
        {
            { GameDuration.Short, TimeSpan.FromSeconds(80)},
            { GameDuration.Normal, TimeSpan.FromSeconds(100)},
            { GameDuration.Extended, TimeSpan.FromSeconds(120)},
        };
        public static IReadOnlyDictionary<GameDuration, TimeSpan> PerCreationTimer = new Dictionary<GameDuration, TimeSpan>()
        {
            { GameDuration.Short, TimeSpan.FromSeconds(60)},
            { GameDuration.Normal, TimeSpan.FromSeconds(70)},
            { GameDuration.Extended, TimeSpan.FromSeconds(90)},
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
            { GameDuration.Normal, 6},
            { GameDuration.Extended, 8},
        };

        public static IReadOnlyDictionary<GameDuration, int> NumCAMsPerPlayer = new Dictionary<GameDuration, int>()
        {
            { GameDuration.Short, 4},
            { GameDuration.Normal, 5},
            { GameDuration.Extended, 6},
        };
    }
}
