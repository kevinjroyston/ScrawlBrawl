using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using RoystonGame.TV;
using RoystonGame.Web.DataModels.UnityObjects;
using RoystonGame.Web.Helpers.Telemetry;
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
        private GameManager GameManager { get; set; }
        private ILogger<UnityHub> Logger { get; set; }
        private RgEventSource EventSource { get; set; }
        public UnityHub(GameManager gameManager, ILogger<UnityHub> logger, RgEventSource eventSource)
        {
            GameManager = gameManager;
            Logger = logger;
            EventSource = eventSource;
        }

        // TODO: this class needs some work.
        public override async Task OnConnectedAsync()
        {
            EventSource.SignalRConnectCounter.WriteMetric(1);
            await Task.Yield();
        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            if( exception != null)
            {
                Logger.LogWarning(exception: exception, message: "Error during signalR connection.");
            }
            EventSource.SignalRDisconnectCounter.WriteMetric(1);
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
                Logger.LogWarning(exception: e, message: $"Error joining lobby as unity client: '{lobbyFriendlyName}'");
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
