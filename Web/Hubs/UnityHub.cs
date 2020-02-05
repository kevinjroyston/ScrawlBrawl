using Microsoft.AspNetCore.SignalR;
using RoystonGame.TV;
using RoystonGame.Web.DataModels.UnityObjects;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoystonGame.Web.Hubs
{
    /// <summary>
    /// <see cref="GameNotifier"/> the background process which uses this Hub.
    /// </summary>
    public class UnityHub : Hub
    {
        // TODO: this class needs some work.
        public override async Task OnConnectedAsync()
        {
            Console.WriteLine("Client connected via SignalR");
            await Task.Yield();
        }
        public void JoinRoom(string lobbyFriendlyName)
        {
            try
            {
                Lobby lobby = GameManager.GetLobby(lobbyFriendlyName);
                if (lobby != null)
                {
                    LeaveAllGroups();
                    Groups.AddToGroupAsync(Context.ConnectionId, lobby.LobbyId);

                    UnityView view = lobby.GetActiveUnityView();
                    Clients.Caller.SendAsync("UpdateState", view);
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
            }
        }
        private void LeaveAllGroups()
        {
            List<Task> tasks = new List<Task>();
            foreach (Lobby lobby in GameManager.GetLobbies())
            {
                tasks.Add(Groups.RemoveFromGroupAsync(Context.ConnectionId, lobby.LobbyId));
            }
            Task.WaitAll(tasks.ToArray());
        }
    }
}
