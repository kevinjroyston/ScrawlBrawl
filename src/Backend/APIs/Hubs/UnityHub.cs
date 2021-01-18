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
        private const string ServerVersion = "1.0.0";


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

        public void ConnectToLobby(string lobbyFriendlyName, string clientVersion)
        {
            double truncatedServerVersion = 0;
            double truncatedClientVersion = 0;
            if (!TryTruncateVersionString(ServerVersion, out truncatedServerVersion)
                || !TryTruncateVersionString(clientVersion, out truncatedClientVersion)
                || truncatedClientVersion < truncatedServerVersion)
            {
                Clients.Caller.SendAsync("UpdateState", JsonConvert.SerializeObject(CommonUnityViews.GenerateInvalidVersionView()));
                return;
            }


            try
            {
                Lobby lobby = GameManager.GetLobby(lobbyFriendlyName);
                if (lobby != null)
                {
                    LeaveAllGroups();
                    Groups.AddToGroupAsync(Context.ConnectionId, lobby.LobbyId);

                    UnityView view = lobby.GetActiveUnityView(returnNullIfNoChange: false);

                    // SignalR's serialization is abysmal and client has no insight into the issue. pull serialization out.
                    Clients.Caller.SendAsync("UpdateState", JsonConvert.SerializeObject(view));
                    Clients.Caller.SendAsync("ConfigureMetadata", JsonConvert.SerializeObject(lobby.ConfigMetaData));
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
            }
        }
        public void ConnectWebLobby(string lobbyAndVersion)
        {  // webviewer can only send in a single string, this has "lobbyid-clientversion"
            string[] webParams = lobbyAndVersion.Split("-");
            if (webParams.Length > 1)
            {
                ConnectToLobby(webParams[0], webParams[1]);
            }
        }
        public void JoinRoom(string versionLobbyString)
        {
            Clients.Caller.SendAsync("UpdateState", CommonUnityViews.GenerateInvalidVersionLegacyView());   
        }
        private bool TryTruncateVersionString(string version, out double result)
        {
            int lastDot = version.LastIndexOf('.');
            if (lastDot <= 0)
            {
                result = 0;
                return false;
            }
            return double.TryParse(version.Substring(0, lastDot), out result);
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
