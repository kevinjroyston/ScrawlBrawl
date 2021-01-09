namespace Backend.Games.TimsGames.FriendQuiz.DataModels
{
    public static class FriendQuizConstants
    {
        public const int PointsForCorrectAnswer = 300;
        public const int PointsForExtraRound = 100;
        public const float ExtraRoundAbstainPercentLimit = 0.75f;
        public const int SliderTickRange = 100;

        public const double SetupTimerMin = 30;
        public const double SetupTimerAve = 60;
        public const double SetupTimerMax = 90;

        public const double AnsweringTimerMin = 15;
        public const double AnsweringTimerAve = 30;
        public const double AnsweringTimerMax = 60;

        public const double VotingTimerMin = 20;
        public const double VotingTimerAve = 30;
        public const double VotingTimerMax = 60;
    }
}
