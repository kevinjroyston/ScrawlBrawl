﻿using Assets.Scripts.Networking.DataModels.UnityObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Networking.DataModels
{
    public class SliderDataHolder
    {
        public (float, float) SliderBounds { get; set; }
        public List<(float, string)> TickLabels { get; set; }
        public List<SliderValueHolder> MainSliderHolders { get; set; }
        public List<SliderValueHolder> GuessSliderHolders { get; set; }
    }
}
