﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.APIs.DataModels.Enums
{
    public enum UnityViewOptions
    {
        // I dont think this works on unity side, dont use it
        //PrimaryAxisMaxCount = 0,
        PrimaryAxis = 1,   // 0 = Horizontal, 1 = Vertical, Defaults to Horizontal (on the game side)
        BlurAnimate = 2,
    }
}
