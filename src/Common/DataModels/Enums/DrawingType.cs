using System;
using System.Collections.Generic;
using System.Text;

namespace Common.DataModels.Enums
{
    /* IMPORTANT: If you add a DrawingType here, you MUST also create a corresponding entry on the front end array:
     *     export const galleryTypes:ReadonlyArray<GalleryType>
     *     in
     *     /app/core/models/gallerytypes.ts
     *     even if the Gallery option will not be used (to define the width and height)
     */
    public enum DrawingType
    {
        Generic,
        Head,
        Body,
        Legs,
        Profile
    }
    public enum SliderTooltipType
    {
        Hide,
        Show,
        Always
    }

}
