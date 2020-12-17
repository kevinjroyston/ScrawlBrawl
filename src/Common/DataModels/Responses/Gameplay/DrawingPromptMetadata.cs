using System.Collections.Generic;
using System.Linq;

namespace Common.DataModels.Responses
{
    /// <summary>
    /// Class containing metadata about a drawing prompt.
    /// </summary>
    public class GalleryOptionMetadata 
    {
        public string GalleryId { get; set; } = "GENERIC";

        public bool GalleryAutoLoadMostRecent { get; set; } = false;
    }
    public class DrawingPromptMetadata
    {
        /// <summary>
        /// Indicates the colors to use in the drawing.
        /// </summary>
        public List<string> ColorList { get; set; }

        public int WidthInPx { get; set; } = 360;
        public int HeightInPx { get; set; } = 360;

        /// <summary>
        /// If included, the base64 png will be rendered on the canvas and returned by the user with modifications.
        /// </summary>
        public string PremadeDrawing { get; set; }

        public GalleryOptionMetadata GalleryOptions { get; set; } 

        /// <summary>
        /// If provided, will be rendered behind the canvas.
        /// </summary>
        public string CanvasBackground { get; set; }
    }
}
