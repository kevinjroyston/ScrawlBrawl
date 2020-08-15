using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.TV.GameModes.KevinsGames.Mimic.DataModels
{
    public static class MimicConstants
    {
        public const int PointsForVote = 100;
        public static int PointsToLooseForForgeting(int numberOfPLayersInLobby)
        {
            return numberOfPLayersInLobby *100;
        }
        public static int PointsForCorrectPick(int numberOfPLayersInLobby)
        {
            return numberOfPLayersInLobby * 20;
        }
        public const double MimicTimerMultiplier = 2.0;

        public const double MemorizeTimerLength = 10;

        public const double BlurDelay = 5.0;
        public const double BlurLength = 15.0;

        public const double SetupTimerMin = 45;
        public const double SetupTimerAve = 120;
        public const double SetupTimerMax = 240;

        public const double DrawingTimerMin = 30;
        public const double DrawingTimerAve = 45;
        public const double DrawingTimerMax = 90;

        public const double VotingTimerMin = 30;
        public const double VotingTimerAve = 60;
        public const double VotingTimerMax = 240;
    }
}
