using Common.DataModels.Enums;
using System;
using System.Collections.Generic;

namespace Backend.Games.TimsGames.FriendQuiz.DataModels
{
    public static class FriendQuizConstants
    {
        public const int PointsForCorrectAnswer = 300;
        public const int PointsForExtraRound = 100;
        public const float ExtraRoundAbstainPercentLimit = 0.75f;
        public const int MaxSliderTickRange = 10000;

        public static IReadOnlyDictionary<GameDuration, TimeSpan> SetupTimer = new Dictionary<GameDuration, TimeSpan>()
        {
            { GameDuration.Short, TimeSpan.FromSeconds(30)},
            { GameDuration.Normal, TimeSpan.FromSeconds(60)},
            { GameDuration.Extended, TimeSpan.FromSeconds(90)},
        };
        public static IReadOnlyDictionary<GameDuration, TimeSpan> AnsweringTimer = new Dictionary<GameDuration, TimeSpan>()
        {
            { GameDuration.Short, TimeSpan.FromSeconds(20)},
            { GameDuration.Normal, TimeSpan.FromSeconds(40)},
            { GameDuration.Extended, TimeSpan.FromSeconds(60)},
        };
        public static IReadOnlyDictionary<GameDuration, TimeSpan> VotingTimer = new Dictionary<GameDuration, TimeSpan>()
        {
            { GameDuration.Short, TimeSpan.FromSeconds(30)},
            { GameDuration.Normal, TimeSpan.FromSeconds(45)},
            { GameDuration.Extended, TimeSpan.FromSeconds(75)},
        };

        // If you have more than this many players on this mode you WILL see impact.
        public static IReadOnlyDictionary<GameDuration, int> MaxUserRounds = new Dictionary<GameDuration, int>()
        {
            { GameDuration.Short, 6},
            { GameDuration.Normal, 12},
            { GameDuration.Extended, 20},
        };
    }
}
