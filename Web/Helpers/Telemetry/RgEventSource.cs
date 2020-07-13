using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.Web.Helpers.Telemetry
{
    // Give your event sources a descriptive name using the EventSourceAttribute, otherwise the name of the class is used. 
    [EventSource(Name = "Application")]
    public sealed class RgEventSource : EventSource
    {
        private EventCounter requestCounter;

        private RgEventSource() : base(EventSourceSettings.EtwSelfDescribingEventFormat)
        {
            this.requestCounter = new EventCounter("request", this);
        }

    }
}
