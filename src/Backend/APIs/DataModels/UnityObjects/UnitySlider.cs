using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.APIs.DataModels.UnityObjects
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
        public IReadOnlyList<(float, string)> TickLabels { get; set; }
        public IReadOnlyList<SliderValueHolder> MainSliderValues { get; set; }
        public IReadOnlyList<SliderValueHolder> GuessSliderValues { get; set; }


        public UnitySlider()
        {
            this.Type = UnityObjectType.Slider;
        }
    }
}
