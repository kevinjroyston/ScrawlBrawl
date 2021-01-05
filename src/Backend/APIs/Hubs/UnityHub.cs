using Microsoft.AspNetCore.SignalR;
using Backend.GameInfrastructure;
using Backend.APIs.DataModels.UnityObjects;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Backend.Games.Common;

namespace Backend.APIs.Hubs
{
    /// <summary>
    /// <see cref="GameNotifier"/> the background process which uses this Hub.
    /// </summary>
    public class UnityHub : Hub
    {
        private const double ServerVersion = 1.0;

        private bool VersionRegistered = false;

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
            if (!VersionRegistered)
            {
                Clients.Caller.SendAsync("UpdateState", JsonConvert.SerializeObject(CommonUnityViews.GenerateInvalidVersionView(ServerVersion)));
                return;
            }

            try
            {
                Lobby lobby = GameManager.GetLobby(lobbyFriendlyName);
                if (lobby != null)
                {
                    LeaveAllGroups();
                    Groups.AddToGroupAsync(Context.ConnectionId, lobby.LobbyId);

                    UnityView view = lobby.GetActiveUnityView(returnNullIfNoChange:false);

                    // SignalR's serialization is abysmal and client has no insight into the issue. pull serialization out.
                    Clients.Caller.SendAsync("UpdateState", JsonConvert.SerializeObject(view));
                    Clients.Caller.SendAsync("ConfigureMetadata", lobby.ConfigMetaData);
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
            }
        }
        public void RegisterVersion(double unityVersion)
        {
            if ((int)unityVersion < (int)ServerVersion) //compares the digits before the point to see if the unity client is out of date
            {
                Clients.Caller.SendAsync("UpdateState", JsonConvert.SerializeObject(CommonUnityViews.GenerateInvalidVersionView(ServerVersion, unityVersion)));
            }
            else
            {
                VersionRegistered = true;
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
