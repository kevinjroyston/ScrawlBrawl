using System;

namespace RoystonGame.TV
{
    public static class Constants
    {
        /// <summary>
        /// The default amount of time to add when trying to avoid state transition race conditions.
        /// </summary>
        public static TimeSpan DefaultBufferTime { get; } = TimeSpan.FromMilliseconds(25.0);

        /// <summary>
        /// The amount of time to wait before considering a user "Inactive"
        /// </summary>
        public static TimeSpan UserInactivityTimer { get; } = TimeSpan.FromSeconds(10.0);
    }
}
