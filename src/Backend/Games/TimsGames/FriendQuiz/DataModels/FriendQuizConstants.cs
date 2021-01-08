namespace Backend.Games.TimsGames.FriendQuiz.DataModels
{
    public static class FriendQuizConstants
    {
        public const int PointsForCorrectAnswer = 100;
        public const int PointsForExtraRound = 100;
        public const float ExtraRoundAbstainPercentLimit = 0.75f;
        public const int SliderTickRange = 100;
        public const int MaxPointsForCorrectGuess = 200;

        public const double SetupTimerMin = 15;
        public const double SetupTimerAve = 30;
        public const double SetupTimerMax = 60;

        public const double AnsweringTimerMin = 15;
        public const double AnsweringTimerAve = 30;
        public const double AnsweringTimerMax = 60;

        public const double VotingTimerMin = 20;
        public const double VotingTimerAve = 30;
        public const double VotingTimerMax = 60;
    }
}
