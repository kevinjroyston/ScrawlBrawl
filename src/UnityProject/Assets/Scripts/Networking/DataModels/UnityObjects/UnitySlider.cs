using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Networking.DataModels.UnityObjects
{
    public class SliderValueHolder
    {
        public float? SingleValue { get; set; }
        public (float?, float?) ValueRange { get; set; }
    }
    public class UnitySlider : UnityObject
    {
        public SliderValueHolder MainSliderValue { get; set; }
        public IReadOnlyList<SliderValueHolder> GuessSliderValues { get; set; }
    }
}
