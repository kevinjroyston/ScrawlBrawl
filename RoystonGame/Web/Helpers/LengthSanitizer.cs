using System;

namespace RoystonGame.Web.Helpers
{
    public class LengthSanitizerAttribute : Attribute
    {
        public int? Min { get; private set; }
        public int? Max { get; private set; }
        public LengthSanitizerAttribute(int min = -1, int max = -1)
        {
            Min = (min < 0) ? null : (int?)min;
            Max = (max < 0) ? null : (int?)max;
        }
    }
}