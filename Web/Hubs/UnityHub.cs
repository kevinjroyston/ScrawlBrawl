using Microsoft.AspNetCore.SignalR;
using RoystonGame.TV;
using RoystonGame.TV.GameEngine;
using RoystonGame.Web.DataModels.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.Web.Hubs
{
    /// <summary>
    /// <see cref="GameNotifier"/> the background process which uses this Hub.
    /// </summary>
    public class UnityHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            Console.WriteLine("Client connected via SignalR");
            // TODO: lobby id logic here?
            await Clients.All.SendAsync("UpdateState", GameManager.GetActiveUnityView());
        }
    }
}
