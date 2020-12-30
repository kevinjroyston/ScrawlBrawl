namespace Backend.Games.BriansGames.ImposterDrawing.DataModels
{
    public static class ImposterDrawingConstants
    {
        // Freebie - Lost*NumVotes will be clipped to be >= 0
        public const int FreebiePointsForNormal = 150;
        public const int LostPointsForBadNormal = 50;

        public const int PointsForCorrectAnswer = 500;
        public const int BonusPointsForGoodImposter = 300;

        public const double SetupTimerMin = 45;
        public const double SetupTimerAve = 120;
        public const double SetupTimerMax = 240;

        public const double AnsweringTimerMin = 45;
        public const double AnsweringTimerAve = 120;
        public const double AnsweringTimerMax = 240;

        public const double VotingTimerMin = 30;
        public const double VotingTimerAve = 60;
        public const double VotingTimerMax = 120;
    }
}
