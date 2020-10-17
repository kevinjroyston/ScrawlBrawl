using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Backend.GameInfrastructure;
using Backend.APIs.DataModels.Enums;
using Backend.APIs.DataModels.UnityObjects;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Backend.APIs.Hubs
{
    /// <summary>
    /// This class runs on a separate thread and checks for state updates, notifying the clients if there are any changes to the gamestate.
    /// </summary>
    public class GameNotifier : IHostedService, IDisposable
    {
        private Timer _timer;
        private IHubContext<UnityHub> UnityHubNotifier { get; }

        private GameManager GameManager { get; set; }

        /// <summary>
        /// Class which runs on a background thread constantly sending updates to connected unity game clients.
        /// </summary>
        /// <param name="notificationHub">The hub to use for pushing updates to clients (Depency Injected)</param>
        public GameNotifier(IHubContext<UnityHub> notificationHub, GameManager gameManager)
        {
            GameManager = gameManager;
            UnityHubNotifier = notificationHub;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(DoWork, null, TimeSpan.Zero,
                TimeSpan.FromSeconds(1));

            return Task.CompletedTask;
        }

        /// <summary>
        /// This function contains the bulk of the work and gets run on a loop.
        /// </summary>
        private void DoWork(object _)
        {
            while (GameManager.AbandonedLobbyIds.TryTake(out string lobbyId))
            {
                UnityHubNotifier.Clients.Group(lobbyId).SendAsync("LobbyClose");
            }

            foreach (Lobby lobby in GameManager.GetLobbies())
            {
                try
                {
                    // Gets the Unity view of the active GameState.
                    UnityView view = lobby.GetActiveUnityView();

                    // Refresh will re-fetch all dynamic values. Returning true if they changed from last fetch.
                    bool needToRefresh = view?.Refresh() ?? false;

                    // Check if we need to send data over the wire.
                    if (needToRefresh)
                    {
                        // Push updates to all clients.
                        UnityHubNotifier.Clients.Group(lobby.LobbyId).SendAsync("UpdateState", view);
                    }


                    bool needToRefreshMetadata = lobby.ConfigMetaData?.Refresh() ?? false;

                    if (needToRefreshMetadata)
                    {
                        UnityHubNotifier.Clients.Group(lobby.LobbyId).SendAsync("ConfigureMetadata", lobby.ConfigMetaData);
                    }
                }
                catch (Exception e)
                {
                    GameManager.ReportGameError(ErrorType.UnityClient, lobby.LobbyId, null, e);
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
