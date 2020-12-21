using Common.DataModels.Enums;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Common.DataModels.Responses
{
    /// <summary>
    /// Class containing metadata about a drawing prompt.
    /// </summary>
    public class GalleryOptionsMetadata 
    {
        public bool GalleryAutoLoadMostRecent { get; set; } = false;
    }

    public class DrawingPromptMetadata
    {
        [JsonProperty(PropertyName = "drawingType")]
        public string DrawingTypeString { get { return this.DrawingType.ToString().ToUpper(); } }

        [JsonIgnore]
        public DrawingType DrawingType { get; set; } = DrawingType.Generic;

        /// <summary>
        /// Indicates the colors to use in the drawing.
        /// </summary>
        public List<string> ColorList { get; set; }

//        public int WidthInPx { get; set; } = 360;
//        public int HeightInPx { get; set; } = 360;

        /// <summary>
        /// If included, the base64 png will be rendered on the canvas and returned by the user with modifications.
        /// </summary>
        public string PremadeDrawing { get; set; }

        public GalleryOptionsMetadata GalleryOptions { get; set; } = new GalleryOptionsMetadata();

        /// <summary>
        /// If provided, will be rendered behind the canvas.
        /// </summary>
        public string CanvasBackground { get; set; }
    }
}
