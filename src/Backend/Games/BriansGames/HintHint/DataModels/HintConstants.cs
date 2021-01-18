using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Games.BriansGames.HintHint.DataModels
{
    public class HintConstants
    {
        public const int NumHintGuessesToShow = 7;
        public const int PointsForGuessingReal = 500;
        public const int PointsForHintingReal = 250;
        public const int PointsForHintingFake = 500;
        public const int PointsForFailingReal = 100;

        public const double SetupRound1TimerMin = 45;
        public const double SetupRound1TimerAve = 90;
        public const double SetupRound1TimerMax = 180;

        public const double SetupRound2TimerMin = 20;
        public const double SetupRound2TimerAve = 45;
        public const double SetupRound2TimerMax = 60;

        public const double SetupRound3TimerMin = 30;
        public const double SetupRound3TimerAve = 60;
        public const double SetupRound3TimerMax = 90;

        public const double GuessingTimerMin = 60;
        public const double GuessingTimerAve = 120;
        public const double GuessingTimerMax = 240;
    }
}
