using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.Web.Helpers.Telemetry
{
    public static class EventIds
    {
        public static EventId Error { get; } = new EventId(7200, "Error");
        public static EventId Startup { get; } = new EventId(7201, "Startup");
    }
}
