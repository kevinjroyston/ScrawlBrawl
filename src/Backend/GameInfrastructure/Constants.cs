using System;

namespace Backend.GameInfrastructure
{
    public static class Constants
    {
        /// <summary>
        /// The default amount of time to add when trying to avoid state transition race conditions.
        /// </summary>
        public static TimeSpan DefaultBufferTime { get; } = TimeSpan.FromMilliseconds(25.0);

        /// <summary>
        /// The amount of time to wait (after fetch current content) before considering a user "Disconnected"
        /// 
        /// Given browsers seem to stop pinging when the user switches tabs, keeping this value relatively high
        /// </summary>
#if !DEBUG
        public static TimeSpan UserDisconnectTimer { get; } = TimeSpan.FromSeconds(31.0);
#else
        // TODO move this to use app settings
        public static TimeSpan UserDisconnectTimer { get; } = TimeSpan.FromSeconds(301.0);
#endif

        /// <summary>
        /// The amount of time to wait after not seeing any user input but still getting pinged to consider them "Inactive", they must also miss the previous submit..
        /// This should be aggressively high.
        /// </summary>
#if !DEBUG
        public static TimeSpan UserInactivityTimer { get; } = TimeSpan.FromSeconds(61.0);
#else
        // TODO move this to use app settings
        // Longer timeouts for debug builds
        public static TimeSpan UserInactivityTimer { get; } = TimeSpan.FromSeconds(601.0);
#endif

        /// <summary>
        /// The amount of time to wait (after last submit) before deleting the user.
        /// </summary>
        public static TimeSpan UserSubmitInactivityTimer { get; } = TimeSpan.FromMinutes(20.0);

        /// <summary>
        /// The minimum amount of time a client should wait between refreshes.
        /// </summary>
        public static TimeSpan MinimumRefreshTime { get; } = TimeSpan.FromMilliseconds(100.0);

        // TODO: States such as Mimic's memorize will use this entire buffer since nobody will be submitting.
        // Need a change to not add the AutoSubmitBuffer for states where we won't be waiting for user input.

        /// <summary>
        /// The amount of buffer to leave between auto submit and the state actually timing out.
        /// </summary>
        public static TimeSpan AutoSubmitBuffer { get; } = TimeSpan.FromSeconds(2.0);

        public static class UIStrings
        {
            public static string DrawingPromptTitle = "Time to draw!";
            public static string SetupUnityTitle = "Setup Time!";
            public static string VotingUnityTitle = "Voting Time!";
            public static string QueryUnityTitle = "Answer These Questions!";
        }
    }
}
