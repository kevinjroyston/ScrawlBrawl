using Common.DataModels.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Backend.Games.Common.ThreePartPeople.DataModels.Person;

namespace Backend.Games.Common.ThreePartPeople.Extensions
{
    public static class BodyPartTypeExtensions
    {
        public static DrawingType GetDrawingType(this BodyPartType bodyPartType)
        {
            switch (bodyPartType)
            {
                case BodyPartType.Head: return DrawingType.Head;
                case BodyPartType.Body: return DrawingType.Body;
                case BodyPartType.Legs: return DrawingType.Legs;
                default: return DrawingType.Generic;
            }
        }
    }
}
