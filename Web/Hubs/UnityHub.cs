using Microsoft.AspNetCore.SignalR;
using RoystonGame.TV.GameEngine;
using RoystonGame.Web.DataModels.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.Web.Hubs
{
    /// <summary>
    /// Default version of this class is sufficient. Only creating new instance in case more hub types need to be made.
    /// <see cref="GameNotifier"/> the background process which uses this Hub.
    /// </summary>
    public class UnityHub : Hub
    {
    }
}
