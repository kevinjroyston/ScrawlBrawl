﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.Web.DataModels.Enums
{
    public enum TVScreenId
    {
        Unknown,
        NoUnityViewConfigured,
        WaitForLobbyToStart,
        WaitForPartyLeader,
        WaitForUserInputs,
        Scoreboard,
        ShowDrawings,
    }
}
