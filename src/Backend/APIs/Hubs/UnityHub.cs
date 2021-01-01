using Microsoft.AspNetCore.SignalR;
using Backend.GameInfrastructure;
using Backend.APIs.DataModels.UnityObjects;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backend.APIs.Hubs
{
    /// <summary>
    /// <see cref="GameNotifier"/> the background process which uses this Hub.
    /// </summary>
    public class UnityHub : Hub
    {
        private GameManager GameManager { get; set; }
        public UnityHub(GameManager gameManager)
        {
            GameManager = gameManager;
        }

        // TODO: this class needs some work.
        public override async Task OnConnectedAsync()
        {
            await Task.Yield();
        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
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

                    Legacy_UnityView view = lobby.GetActiveUnityView();
                    Clients.Caller.SendAsync("UpdateState", view);
                    Clients.Caller.SendAsync("ConfigureMetadata", lobby.ConfigMetaData);
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
