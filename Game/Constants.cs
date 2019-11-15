using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.Game
{
    public class Constants
    {
        /// <summary>
        /// The default amount of time to add when trying to avoid state transition race conditions.
        /// </summary>
        public static TimeSpan DefaultBufferTime { get; } = TimeSpan.FromMilliseconds(25.0);
    }
}
