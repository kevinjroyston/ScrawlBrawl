using System;

namespace RoystonGame.TV
{
    public static class Constants
    {
        /// <summary>
        /// The default amount of time to add when trying to avoid state transition race conditions.
        /// </summary>
        public static TimeSpan DefaultBufferTime { get; } = TimeSpan.FromMilliseconds(25.0);
    }
}
