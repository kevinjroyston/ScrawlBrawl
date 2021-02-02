using Common.DataModels.Enums;
using Newtonsoft.Json;

namespace Common.DataModels.Responses
{

    public class RangeHighlightsType
    {
        public int Start { get; set; }
        public int End { get; set; }
        public string Class { get; set; }
    }
    /// <summary>
    /// Class containing metadata about a selector prompt.
    /// </summary>
    public class SliderPromptMetadata
    {
        /// <summary>
        /// Slider range: MinValue MaxValue, CurrentValue
        /// MinLabel and MaxLabel show above the selector
        /// </summary>
        public int Min { get; set; } = 0;
        public int Max { get; set; } = 100;
        public int[] Value { get; set; } = new int[] { 50 };
        public bool Range { get; set; }
        public int[] Ticks { get; set; }
        public bool Enabled { get; set; } = true;

        [JsonProperty(PropertyName = "showTooltip")]
        public string ShowTooltipString{ get { return this.ShowTooltip.ToString().ToLower(); } }

        [JsonIgnore]
        public SliderTooltipType ShowTooltip { get; set; } = SliderTooltipType.Hide;

        public RangeHighlightsType[] RangeHighlights { get; set; }
        public string[] TicksLabels { get; set; }

    }
}
