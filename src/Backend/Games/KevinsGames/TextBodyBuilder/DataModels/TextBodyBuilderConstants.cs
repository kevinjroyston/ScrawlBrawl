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
            { GameDuration.Short, TimeSpan.FromSeconds(30)},
            { GameDuration.Normal, TimeSpan.FromSeconds(35)},
            { GameDuration.Extended, TimeSpan.FromSeconds(35)},
        };
        public static IReadOnlyDictionary<GameDuration, TimeSpan> SetupPerPromptTimer = new Dictionary<GameDuration, TimeSpan>()
        {
            { GameDuration.Short, TimeSpan.FromSeconds(65)},
            { GameDuration.Normal, TimeSpan.FromSeconds(75)},
            { GameDuration.Extended, TimeSpan.FromSeconds(85)},
        };
        public static IReadOnlyDictionary<GameDuration, TimeSpan> PerCreationTimer = new Dictionary<GameDuration, TimeSpan>()
        {
            { GameDuration.Short, TimeSpan.FromSeconds(70)},
            { GameDuration.Normal, TimeSpan.FromSeconds(80)},
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
            { GameDuration.Short, 6},
            { GameDuration.Normal, 7},
            { GameDuration.Extended, 8},
        };
    }
}
