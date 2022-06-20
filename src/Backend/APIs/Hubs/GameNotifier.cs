using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Backend.GameInfrastructure;
using Backend.APIs.DataModels.Enums;
using Backend.APIs.DataModels.UnityObjects;
using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Linq;

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
                    UnityRPCRequestHolder unityRPCHolder = new UnityRPCRequestHolder();

                    unityRPCHolder.UnityView = lobby.GetActiveUnityView(returnNullIfNoChange: true);
                    bool needToRefreshMetadata = lobby.ConfigMetaData?.Refresh() ?? false;
                    unityRPCHolder.ConfigurationMetadata = (needToRefreshMetadata) ? lobby.ConfigMetaData:null;
                    unityRPCHolder.UnityImageList = new UnityImageList();
                    if (unityRPCHolder.UnityView?.UnityObjects?.Value != null)
                    {
                        foreach(var unityObject in unityRPCHolder.UnityView.UnityObjects.Value)
                        {
                            var unityImage = unityObject as UnityImage;
                            if (unityImage != null)
                            {
                                foreach (var drawingObject in unityImage.DrawingObjects.Where(drawing => !drawing.SentToClient))
                                {
                                    unityRPCHolder.UnityImageList.ImgList.Add(drawingObject.Id.ToString(),drawingObject.DrawingStr);
                                    drawingObject.MarkSentToClient();
                                    lobby.AddDrawingObjectToRepository(drawingObject);

                                }
                            }
                        }
                    }
                    if (unityRPCHolder.UnityView?.Users != null)
                    {
                        foreach (var unityUser in unityRPCHolder.UnityView.Users.Where(user=>!user.DrawingObject.SentToClient))
                        {
                            unityRPCHolder.UnityImageList.ImgList.Add(unityUser.DrawingObject.Id.ToString(), unityUser.DrawingObject.DrawingStr);
                            unityUser.DrawingObject.MarkSentToClient();
                            lobby.AddDrawingObjectToRepository(unityUser.DrawingObject);
                        }
                    }

                    if (!unityRPCHolder.UnityImageList.ImgList.Any())
                    {
                        unityRPCHolder.UnityImageList = null;
                    }

                    bool unityUserStatusChanged = lobby.HasUnityUserStatusChanged();

                    // SignalR's serialization is abysmal and client has no insight into the issue. pull serialization out.
                    if (unityUserStatusChanged || (unityRPCHolder.UnityView != null) || (unityRPCHolder.ConfigurationMetadata != null) || (unityRPCHolder.UnityImageList != null))
                    {
                        unityRPCHolder.UnityUserStatus = lobby.GetUsersAnsweringPrompts();

                        UnityHubNotifier.Clients.Group(lobby.LobbyId).SendAsync("UpdateState", JsonConvert.SerializeObject(unityRPCHolder, Formatting.None, SerializerSettings));
                    }


                }
                catch (Exception e)
                {
                    GameManager.ReportGameError(ErrorType.UnityClient, lobby.LobbyId, null, e);
                }
            }
        }

        private static JsonSerializerSettings SerializerSettings = new JsonSerializerSettings()
        {
            DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate
        };

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
