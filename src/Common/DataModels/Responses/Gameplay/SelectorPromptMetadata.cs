namespace Common.DataModels.Responses
{

    /// <summary>
    /// Class containing metadata about a selector prompt.
    /// </summary>
    public class SelectorPromptMetadata
    {
        /// <summary>
        /// Height and width of images in the selector list
        /// </summary>
        public int WidthInPx { get; set; } = 350;
        public int HeightInPx { get; set; } = 350;

        /// <summary>
        /// base64 png will be rendered as each image in the list
        /// </summary>
        public string[] ImageList { get; set; }
    }
}
