using System;

namespace Common.Code.Extensions
{
    public static class TimeSpanExtensions
    {
        public static TimeSpan? MultipliedBy(this TimeSpan? originalTimeSpan, float multiplier)
        {
            if (originalTimeSpan == null)
            {
                return null;
            }
            return TimeSpan.FromTicks(((TimeSpan)originalTimeSpan).Ticks * (long)multiplier);
        }
    }
}
