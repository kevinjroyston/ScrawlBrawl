using System.Collections.Generic;

namespace RoystonGame.TV.GameModes.BriansGames.BattleReady.DataModels
{
    public static class BattleReadyConstants
    {
        public const int PointsForVote = 100;
        public const int PointsForPartUsed = 100;
        public const int PointMultiplierForPromptRating = 50; // ranges from * -2 to  * 2

        public const double SetupDrawingTimerMin = 120;
        public const double SetupDrawingTimerAve = 240;
        public const double SetupDrawingTimerMax = 480;

        public const double SetupPromptTimerMin = 60;
        public const double SetupPromptTimerAve = 180;
        public const double SetupPromptTimerMax = 360;

        public const double CreationTimerMin = 45;
        public const double CreationTimerAve = 120;
        public const double CreationTimerMax = 240;

        public const double VotingTimerMin = 30;
        public const double VotingTimerAve = 60;
        public const double VotingTimerMax = 120;
    }
}
