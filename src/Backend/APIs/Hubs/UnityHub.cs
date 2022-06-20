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
        private const string ServerVersion = "3.0.0";


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
                UnityRPCRequestHolder unityRPCHolder = new UnityRPCRequestHolder();

                unityRPCHolder.UnityView = CommonUnityViews.GenerateInvalidVersionView();
                
                // SignalR's serialization is abysmal and client has no insight into the issue. So we serialization before.
                Clients.Caller.SendAsync("UpdateState", JsonConvert.SerializeObject(unityRPCHolder, SerializerSettings));
                return;
            }


            try
            {
                Lobby lobby = GameManager.GetLobby(lobbyFriendlyName);
                if (lobby != null)
                {
                    LeaveAllGroups();
                    Groups.AddToGroupAsync(Context.ConnectionId, lobby.LobbyId);
                    UnityRPCRequestHolder unityRPCHolder=new UnityRPCRequestHolder();

                    /* joining a lobby, send everything down */
                    unityRPCHolder.UnityView = lobby.GetActiveUnityView(returnNullIfNoChange: false);
                    unityRPCHolder.ConfigurationMetadata = lobby.ConfigMetaData;
                    unityRPCHolder.UnityImageList = lobby.LobbyImageList;
                    unityRPCHolder.UnityUserStatus = lobby.GetUsersAnsweringPrompts();


                    // SignalR's serialization is abysmal and client has no insight into the issue. So we serialization before.
                    Clients.Caller.SendAsync("UpdateState", JsonConvert.SerializeObject(unityRPCHolder,Formatting.None, SerializerSettings));
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
            }
        }

        private static JsonSerializerSettings SerializerSettings = new JsonSerializerSettings()
        {
            DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate
        };
        public void ConnectWebLobby(string lobbyAndVersion)
        {  // webviewer can only send in a single string, this has "lobbyid-clientversion"
            string[] webParams = lobbyAndVersion.Split("-");
            if (webParams.Length > 1)
            {
                ConnectToLobby(webParams[0], webParams[1]);
            }
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
