using System.Collections.Generic;
using System.Linq;

namespace Common.DataModels.Responses
{
    /// <summary>
    /// Class containing metadata about a drawing prompt.
    /// </summary>
    public class DrawingPromptMetadata
    {
        /// <summary>
        /// Indicates the colors to use in the drawing.
        /// </summary>
        public List<string> ColorList { get; set; } = Constants.DefaultColorPalette.ToList();

        public int WidthInPx { get; set; } = 350;
        public int HeightInPx { get; set; } = 350;

        /// <summary>
        /// If included, the base64 png will be rendered on the canvas and returned by the user with modifications.
        /// </summary>
        public string PremadeDrawing { get; set; }

        /// <summary>
        /// If provided, will be rendered behind the canvas.
        /// </summary>
        public string CanvasBackground { get; set; }
    }
}
