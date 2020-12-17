using Common.DataModels.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Backend.Games.Common.ThreePartPeople.DataModels.Person;

namespace Backend.Games.Common.ThreePartPeople.Extensions
{
    public static class DrawingTypeExtensions
    {
        public static GalleryId GetGalleryId(this DrawingType drawingType)
        {
            switch (drawingType)
            {
                case DrawingType.Head: return GalleryId.Head;
                case DrawingType.Body: return GalleryId.Body;
                case DrawingType.Legs: return GalleryId.Legs;
                default: return GalleryId.Generic;
            }
        }
    }
}
