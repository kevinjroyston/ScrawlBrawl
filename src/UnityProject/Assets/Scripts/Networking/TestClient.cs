using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Assets.Scripts.Networking.DataModels;
using Newtonsoft.Json;
using Assets.Scripts.Networking.DataModels.Enums;
using Newtonsoft.Json.Linq;
using static UnityEngine.UI.GridLayoutGroup;
using Assets.Scripts.Networking.DataModels.UnityObjects;
using System.Collections.Specialized;

/// <summary>
/// This class opens a connection to the server and listens for updates. From the main thread the secondary connection thread is
/// monitored and restarted as needed. Any updates from the secondary thread will get merged back to the main thread in the update
/// loop before leaving this class.
/// </summary>
public class TestClient : MonoBehaviour
{
    private SignalRLib srLib;

    private const string ClientVersion = "2.1.0";
    private Task hubTask;

    private List<string> handlers = new List<string>() { "ConfigureMetadata", "UpdateState", "LobbyClose" };
    private string signalRHubURL = "";

    private bool Connected { get; set; } = false;

    // Hacky fix to send the update from the main thread.
    private bool Dirty { get; set; } = false;
    private bool LobbyClosed { get; set; } = false;
    private UnityView CurrentView { get; set; }

    private bool ConfigDirty { get; set; } = false;
    private ConfigurationMetadata ConfigurationMeta { get; set; }

    private bool Restarting { get; set; } = false;
    public EnterLobbyId EnterLobbyId;

    /// <summary>
    /// Set up the connection and callbacks.
    /// </summary>
    void Awake()
        {
#if DEBUG
        signalRHubURL = "http://localhost:50403/signalr";

        //signalRHubURL="https://api.test.scrawlbrawl.tv/signalr";

#else
        signalRHubURL="https://api.scrawlbrawl.tv/signalr";
#endif
#if UNITY_WEBGL
        
        if (Application.absoluteURL.Contains("localhost") || Application.absoluteURL=="")
        {
            signalRHubURL = "http://localhost:50403/signalr";
        }
        else if (Application.absoluteURL.Contains("test.")) {
            signalRHubURL="https://api.test.scrawlbrawl.tv/signalr";
        }
        else{
            signalRHubURL = "https://api.scrawlbrawl.tv/signalr";
        }
#endif

        Debug.Log("URL:"+Application.absoluteURL);
        srLib = new SignalRLib(signalRHubURL, handlers, true);

        Application.runInBackground = true;
        QualitySettings.vSyncCount = 0;  // VSync must be disabled

#if UNITY_WEBGL
        Application.targetFrameRate = -1;  // https://docs.unity3d.com/ScriptReference/Application-targetFrameRate.html
#else
        Application.targetFrameRate = 60;
#endif

        srLib.ConnectionStarted += (object sender, ConnectionEventArgs e) =>
        {
            Debug.Log(e.ConnectionId);
            Connected = true;  // just a flag we are using to know we connected, does not ensure we have not been disconnected
            if (!string.IsNullOrEmpty(this.LobbyId))
            {
                ConnectToLobby(this.LobbyId);
            }

/* no longer needed with embedded viewer
            Uri uri = new Uri(Application.absoluteURL);
            string[] lobby = uri.Query.Split(new string[]{"lobby="},StringSplitOptions.None);
            if (lobby.Length==2 && lobby[1].Length > 0)
            {
                ConnectToLobby(lobby[1].Split('&')[0].Truncate(10));
            }
*/
        };

        srLib.HandlerInvoked += (object sender, HandlerEventArgs e) =>
        {
            Debug.Log("handler invoked");

            switch (e.HandlerName)
            {
                case "ConfigureMetadata":
                    ConfigurationMeta = JsonConvert.DeserializeObject<ConfigurationMetadata>(e.Payload);
                    ConfigDirty = true;
                    break;
                case "UpdateState":
                    try {
                        CurrentView = ParseJObjects(JsonConvert.DeserializeObject<UnityView>(e.Payload));
                    }
                    catch (Exception err)
                    {
                        Debug.Log(err.Message);
                    }
                    Dirty = true;
                    break;
                case "LobbyClose":
                    LobbyClosed = true;
                    Dirty = true;
                    break;
                default:
                    Debug.Log($"Handler: '{e.HandlerName}' not defined");
                    break;
            }
        };

        // plr ConnectToHub();
        }

        /// <summary>
        /// Iterates through all "object" dictionaries and parses objects.
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        public UnityView ParseJObjects(UnityView view)
        {
            foreach (UnityViewOptions key in view?.Options?.Keys?.ToList() ?? new List<UnityViewOptions>())
            {
                switch (key)
                {
                    case UnityViewOptions.BlurAnimate:
                        view.Options[key] = ((JObject)view.Options[key]).ToObject<UnityField<float?>>();
                        break;
                    default:
                        break;
                }
            }

            if (view.UnityObjects?.Value != null)
            {
                List<UnityObject> unityObjects = new List<UnityObject>();
                foreach (object obj in view.UnityObjects.Value)
                {
                    JObject jObject = (JObject)obj;
                    switch (jObject["Type"].ToObject<UnityObjectType>())
                    {
                        case UnityObjectType.Image:
                            unityObjects.Add(jObject.ToObject<UnityImage>());
                            break;
                        case UnityObjectType.Slider:
                            unityObjects.Add(jObject.ToObject<UnitySlider>());
                            break;
                        case UnityObjectType.Text:
                            unityObjects.Add(jObject.ToObject<UnityText>());
                            break;
                        default:
                            throw new NotImplementedException("Not implemented");
                    }
                }
            view.UnityObjects.Value = unityObjects.Cast<object>().ToList();
            }
            if (view.UnityObjects?.StartValue != null || view.UnityObjects?.EndValue !=null)
            {
                throw new NotImplementedException("not implemented");
            }

            return view;
        }

        string LobbyId = null;
        public void ConnectToLobby(string lobby)
        {
            LobbyId = lobby;
            if (Connected)
            {
                srLib.SendToHub("ConnectWebLobby", LobbyId + "-" + ClientVersion);
            }
        }

        public void Start()
        {
#region Debug Unity View 
            /// This is paired with ViewManager.SetDebugCustomView() to allow testing of views without having to do anything on backend
            /// To use uncomment out this code, modify the UnityView being passed in and COMMENT OUT THE UPDATE LOOP
            /// When you are done please revert back the comments
            /// ====================================================================================
            /// THIS CODE IS ONLY FOR DEBUGGING PURPOSES AND SHOULD NOT BE CALLED EVER ON PRODUCTION
            /// ====================================================================================

            /*
            List<UnityUser> fakeUsers = new List<UnityUser>()
            {
                new UnityUser()
                {
                    Id = Guid.NewGuid(),
                    DisplayName = "Test User 1",
                    Activity = UserActivity.Active,
                    Status = UserStatus.AnsweringPrompts,
                },
                new UnityUser()
                {
                    Id = Guid.NewGuid(),
                    DisplayName = "Test User 2",
                    Activity = UserActivity.Active,
                    Status = UserStatus.Waiting,
                },
                new UnityUser()
                {
                    Id = Guid.NewGuid(),
                    DisplayName = "Test User 3",
                    Activity = UserActivity.Active,
                    Status = UserStatus.Waiting,
                },
                new UnityUser()
                {
                    Id = Guid.NewGuid(),
                    DisplayName = "Test User 4",
                    Activity = UserActivity.Active,
                    Status = UserStatus.Waiting,
                },
            };
            ViewManager.Singleton.SetDebugCustomView(
                TVScreenId.VoteRevealImageView,
                new UnityView()
                {
                    UnityObjects = new UnityField<IReadOnlyList<object>>
                    {
                        Value = new List<UnitySlider>()
                        {

                            new UnitySlider()
                            {
                                OwnerUserId = fakeUsers[0].Id,
                                Type = UnityObjectType.Slider,
                                SliderBounds = (0, 10),
                                MainSliderValue = new SliderValueHolder()
                                {
                                    UserId = Guid.NewGuid(),
                                    ValueRange = (2,4),
                                },
                                GuessSliderValues = new List<SliderValueHolder>()
                                {
                                    new SliderValueHolder()
                                    {
                                        UserId = Guid.NewGuid(),
                                        SingleValue = 3
                                    },
                                    new SliderValueHolder()
                                    {
                                        UserId = Guid.NewGuid(),
                                        SingleValue = 5
                                    },
                                    new SliderValueHolder()
                                    {
                                        UserId = Guid.NewGuid(),
                                        SingleValue = 9
                                    },
                                },
                                Title = new UnityField<string>()
                                {
                                    Value = "Test Title"
                                },
                                UnityObjectId = Guid.NewGuid(),
                            },
                            new UnitySlider()
                            {
                                OwnerUserId = fakeUsers[0].Id,
                                Type = UnityObjectType.Slider,
                                SliderBounds = (0, 10),
                                MainSliderValue = new SliderValueHolder()
                                {
                                    UserId = Guid.NewGuid(),
                                    SingleValue = 7,
                                },
                                GuessSliderValues = new List<SliderValueHolder>()
                                {
                                    new SliderValueHolder()
                                    {
                                        UserId = Guid.NewGuid(),
                                        ValueRange = (0,10)
                                    },
                                    new SliderValueHolder()
                                    {
                                        UserId = Guid.NewGuid(),
                                        ValueRange = (4,7)
                                    },
                                },
                                Title = new UnityField<string>()
                                {
                                    Value = "Test Title"
                                },
                                UnityObjectId = Guid.NewGuid(),
                            },
                        }
                    },
                    Users = fakeUsers,
                    Title = new UnityField<string>()
                    {
                        Value = "Test Title"
                    },
                    Instructions = new UnityField<string>()
                    {
                        Value = "Test Instructions"
                    },
                    ServerTime = DateTime.UtcNow,
                    StateEndTime = null,
                    IsRevealing = true,
                    Options = new Dictionary<UnityViewOptions, object>()
                });

                 */
#endregion
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
                    ViewManager.Singleton.SwitchToView(CurrentView?.ScreenId ?? TVScreenId.Unknown, CurrentView);
                }
            }

            if (ConfigDirty)
            {
                Debug.Log($"Config Update");
                ConfigDirty = false;
                ViewManager.Singleton.UpdateConfigMetaData(ConfigurationMeta);
            }

            // If we aren't in the process of a delayed restart and the connection task failed. Begin a delayed restart.
            // plr-old if (!Restarting && (hubConnection?.State != HubConnectionState.Connected || hubTask==null || hubTask.IsFaulted || hubTask.IsCanceled))
            // NOTE: the Connected in the line below only says we EVER connected, not that we are still connected
            if (!Restarting && (Connected || hubTask == null || hubTask.IsFaulted || hubTask.IsCanceled))
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
        }

        private void ConnectToHub()
        {
            if(srLib == null)
            {
                return;
            }

            if(hubTask!=null && !(Connected || hubTask.IsFaulted || hubTask.IsCanceled))
            {
                Debug.Log("Hub restart requested but connection is active");
                return;
            }
        }
 
}
