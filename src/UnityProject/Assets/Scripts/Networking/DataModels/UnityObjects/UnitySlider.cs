﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Networking.DataModels.UnityObjects
{
    public class SliderValueHolder
    {
        public float? SingleValue { get; set; }
        public (float, float)? ValueRange { get; set; }
        public Guid UserId { get; set; }
    }
    public class UnitySlider : UnityObject
    {
        public (float, float) SliderBounds { get; set; }
        public List<(float, string)> TickLabels { get; set; }
        public List<SliderValueHolder> MainSliderValues { get; set; }
        public List<SliderValueHolder> GuessSliderValues { get; set; }
    }
}
