namespace RoystonGame.Web.DataModels.Responses
{

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
        public int Value { get; set; } = 0;
        public bool Range { get; set; }
        public int[] Ticks { get; set; }

        public string[] TicksLabels { get; set; }

    }
}
