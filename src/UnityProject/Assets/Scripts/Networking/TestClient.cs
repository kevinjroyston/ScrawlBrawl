using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

/// <summary>
/// This class opens a connection to the server and listens for updates. From the main thread the secondary connection thread is
/// monitored and restarted as needed. Any updates from the secondary thread will get merged back to the main thread in the update
/// loop before leaving this class.
/// </summary>
public class TestClient : MonoBehaviour
{
    private HubConnection hubConnection;
    private Task hubTask;

    // Hacky fix to send the update from the main thread.
    private bool Dirty { get; set; } = false;
    private bool LobbyClosed { get; set; } = false;
    private Legacy_UnityView CurrentView { get; set; }

    private bool ConfigDirty { get; set; } = false;
    private ConfigurationMetadata ConfigurationMeta { get; set; }

    private bool Restarting { get; set; } = false;
    public EnterLobbyId EnterLobbyId;

    /// <summary>
    /// Set up the connection and callbacks.
    /// </summary>
    void Awake()
    {
        Application.runInBackground = true;
        QualitySettings.vSyncCount = 0;  // VSync must be disabled
        Application.targetFrameRate = 60;

        hubConnection = new HubConnectionBuilder()
#if DEBUG
            .WithUrl("http://localhost:50403/signalr")
            //.WithUrl("https://api.test.scrawlbrawl.tv/signalr")

#else
            .WithUrl("https://api.scrawlbrawl.tv/signalr")
#endif
            .ConfigureLogging(logging =>
            {
                logging.AddProvider(new DebugLoggerProvider());
            })
            .Build();

        hubConnection.On("ConfigureMetadata",
            new Action<ConfigurationMetadata>((configMeta) =>
            {
                ConfigurationMeta = configMeta;
                ConfigDirty = true;
            }));

        hubConnection.On("UpdateState",
            new Action<Legacy_UnityView>((view) =>
            {
                CurrentView = view;
                Dirty = true;
            }));

        hubConnection.On("LobbyClose",
            new Action(() =>
            {
                LobbyClosed = true;
                Dirty = true;
            }));

        ConnectToHub();
    }

    string LobbyId = null;
    public void ConnectToLobby(string lobby)
    {
        LobbyId = lobby;
        hubConnection.SendAsync("JoinRoom", lobby);
    }

    public void Update()
    {
        // If the Dirty bit is set that means the networking thread got a response from the server. Since it is not possible
        // to make certain types of calls outside of the main thread we listen for it here and make the call here.
        if (Dirty)
        {
            Debug.Log($"Server update");
            Dirty = false;
            if (LobbyClosed)
            {
                LobbyClosed = false;
                ViewManager.Singleton.OnLobbyClose();

            }
            else
            {
                ViewManager.Singleton.SwitchToView(CurrentView?._ScreenId ?? TVScreenId.Unknown, CurrentView);
            }
        }

        if (ConfigDirty)
        {
            Debug.Log($"Config Update");
            ConfigDirty = false;
            ViewManager.Singleton.UpdateConfigMetaData(ConfigurationMeta);
        }

        // If we aren't in the process of a delayed restart and the connection task failed. Begin a delayed restart.
        if (!Restarting && (hubConnection?.State != HubConnectionState.Connected || hubTask==null || hubTask.IsFaulted || hubTask.IsCanceled))
        {
            StartCoroutine(DelayedConnectToHub());
        }
    }

    /// <summary>
    /// Restarts the connection after a 5 second delay.
    /// </summary>
    /// <returns>A coroutine representing the task.</returns>
    IEnumerator DelayedConnectToHub()
    {
        Restarting = true;
        yield return new WaitForSeconds(5);
        ConnectToHub();
        Restarting = false;
    }

    public void OnApplicationQuit()
    {
        hubConnection?.DisposeAsync();
    }

    private void ConnectToHub()
    {
        if(hubConnection == null)
        {
            return;
        }

        if(hubTask!=null && !(hubConnection?.State != HubConnectionState.Connected || hubTask.IsFaulted || hubTask.IsCanceled))
        {
            Debug.Log("Hub restart requested but connection is active");
            return;
        }

        hubTask = hubConnection
            .StartAsync().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Console.WriteLine("There was an error opening the connection:{0}",
                                      task.Exception.GetBaseException());
                    throw task.Exception.GetBaseException();
                }
                else
                {
                    Console.WriteLine("Connected to Server");
                    if(!string.IsNullOrWhiteSpace(LobbyId))
                    {
                        hubConnection.SendAsync("JoinRoom", LobbyId);
                    }
                    else
                    {
                        Dirty = true;
                        LobbyClosed = true;
                    }
                }
            });
    }
}
