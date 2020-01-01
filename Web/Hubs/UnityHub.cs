using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoystonGame.Web.Hubs
{
    public class UnityHub : Hub
    {
        public UnityHub()
        {
            Console.WriteLine("Initialized instance of Unity Hub");

        }
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public override async Task OnConnectedAsync()
        {
            Console.WriteLine("Client connected via SignalR");
            await SendMessage("test", "test2");
        }
    }
}
