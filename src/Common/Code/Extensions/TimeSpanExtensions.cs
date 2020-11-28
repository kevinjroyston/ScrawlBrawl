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
            return originalTimeSpan.Value.MultipliedBy(multiplier);
        }
        public static TimeSpan MultipliedBy(this TimeSpan originalTimeSpan, float multiplier)
        {
            return TimeSpan.FromTicks((long)(originalTimeSpan.Ticks * multiplier));
        }
    }
}
