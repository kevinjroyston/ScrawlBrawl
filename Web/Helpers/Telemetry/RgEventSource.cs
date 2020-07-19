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
        public EventCounter SignalRConnectCounter { get; }
        public EventCounter SignalRDisconnectCounter { get; }
        public EventCounter LobbyStartCounter { get; }
        public EventCounter LobbyEndCounter { get; }
        public EventCounter GameStartCounter { get; }
        public EventCounter UserCounter { get; }
        public EventCounter GameErrorCounter { get; }

        public RgEventSource() : base(EventSourceSettings.EtwSelfDescribingEventFormat)
        {
            this.LobbyStartCounter = new EventCounter("LobbyStart", this);
            this.LobbyEndCounter = new EventCounter("LobbyEnd", this);
            this.GameStartCounter = new EventCounter("GameStart", this);
            this.SignalRConnectCounter = new EventCounter("SignalRConnect", this);
            this.SignalRDisconnectCounter = new EventCounter("SignalRDisconnect", this);
            this.GameErrorCounter = new EventCounter("GameError", this);
        }
    }
}
