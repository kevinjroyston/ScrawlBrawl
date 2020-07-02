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

        public static int PointsForSpeed(double secondsTaken, int numberOfPLayersInLobby)
        {
            if(secondsTaken <= BlurDelay)
            {
                return PointsForCorrectPick(numberOfPLayersInLobby) * 2;
            }
            else
            {
                return (int)((BlurLength - secondsTaken - BlurDelay) / BlurLength * PointsForCorrectPick(numberOfPLayersInLobby) * 2);
            }
        }

        public const double BlurDelay = 5.0;
        public const double BlurLength = 15.0;
    }
}
