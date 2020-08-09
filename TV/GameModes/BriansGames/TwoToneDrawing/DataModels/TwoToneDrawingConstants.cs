using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.TV.GameModes.BriansGames.TwoToneDrawing.DataModels
{
    public static class TwoToneDrawingConstants
    {
        public static int PointsForMakingWinningDrawing { get; } = 500;
        public static int PointsForVotingForWinningDrawing { get; } = 100;
        public static int PointsToLoseForBadSelfVote { get; } = -100;
    }
}
