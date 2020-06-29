using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.TV.GameModes.KevinsGames.Mimic.DataModels
{
    public static class MimicConstants
    {
        public const int PointsForVote = 100;
        public static int PointsForCorrectPick(int numberOfPLayersInLobby)
        {
            return numberOfPLayersInLobby * 20;
        }
        public const double MimicTimerMultiplier = 1.2;
    }
}
