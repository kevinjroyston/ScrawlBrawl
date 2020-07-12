using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.Web.Helpers.Telemetry
{
    public static class EventIds
    {
        public static EventId Error { get; } = new EventId(0, "Error");
        public static EventId Startup { get; } = new EventId(1, "Startup");
        public static EventId Controller { get; } = new EventId(2, "Controller");
    }
}
