using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.Web.DataModels.Responses
{
    /// <summary>
    /// Class containing metadata about a drawing prompt.
    /// </summary>
    public class DrawingPromptMetadata
    {
        /// <summary>
        /// Indicates the color to use in the drawing. If there is a color picker element it should automatically link to the brush color.
        /// </summary>
        public string Color { get; set; }

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
