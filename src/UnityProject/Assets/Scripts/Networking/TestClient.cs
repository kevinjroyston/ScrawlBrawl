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
using System.IO;
using System.Collections.Concurrent;


/// <summary>
/// This class opens a connection to the server and listens for updates. From the main thread the secondary connection thread is
/// monitored and restarted as needed. Any updates from the secondary thread will get merged back to the main thread in the update
/// loop before leaving this class.
/// </summary>
public class TestClient : MonoBehaviour
{
    private SignalR signalR;

    private const string ClientVersion = "4.0.0";
    private Task hubTask;

    private string signalRHubURL = "";

    private bool Connected { get; set; } = false;

    // Hacky fix to send the update from the main thread.
    private ConcurrentQueue<UnityRPCDataHolder> RPCRequestQueue { get; set; } = new ConcurrentQueue<UnityRPCDataHolder>();
    private bool Dirty { get; set; } = false;
    private bool LobbyClosed { get; set; } = false;

    private bool Restarting { get; set; } = false;
    public EnterLobbyId EnterLobbyId;

    void DumpToLog(string LogInfo)
    {
#if DEBUG
        string path = "c:\\SBLogs\\SBlog.txt";
        if (!Directory.Exists("c:\\SBLogs"))
            return;
        //Write some text to the test.txt file
        StreamWriter writer = new StreamWriter(path, true);
        writer.WriteLine(LogInfo);
        writer.Close();
#endif
    }

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

        Debug.Log("URL:"+Application.absoluteURL + "  Chosen api:"+signalRHubURL);
        // Initialize SignalR
        signalR = new SignalR();
        signalR.Init(signalRHubURL);

        Application.runInBackground = true;
        QualitySettings.vSyncCount = 0;  // VSync must be disabled

#if UNITY_WEBGL
        Application.targetFrameRate = -1;  // https://docs.unity3d.com/ScriptReference/Application-targetFrameRate.html
#else
        Application.targetFrameRate = 60;
#endif

        signalR.ConnectionStarted += (object sender, ConnectionEventArgs e) =>
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



        // Handler callback
        /*signalR.On("ConfigureMetadata", (string payload) =>
        {
            Debug.Log("ConfigureMetadata invoked");
            DumpToLog("ConfigureMetadata");
            DumpToLog(payload);


        });*/  // This still needed?
        signalR.On("UpdateState", (string payload) =>
        {
            Debug.Log("UpdateState invoked");
            DumpToLog("UpdateState");
            DumpToLog(payload);

            try
            {
                var newRPCData = JsonConvert.DeserializeObject<UnityRPCDataHolder>(payload, SerializerSettings);
                if (newRPCData.UnityView != null)
                {
                    newRPCData.UnityView = ParseJObjects(newRPCData.UnityView);
                }
                RPCRequestQueue.Enqueue(newRPCData);
            }
            catch (Exception err)
            {
                Debug.Log(err.ToString());
                Debug.Log(payload);
            }
            Dirty = true;

        });
        signalR.On("LobbyClose", (string payload) =>
        {
            Debug.Log("LobbyClose invoked");
            DumpToLog("LobbyClose");
            DumpToLog(payload);

            LobbyClosed = true;
            Dirty = true;
        });

        signalR.ConnectionClosed += (object sender, ConnectionEventArgs e) =>
        {
            // Log the disconnected ID
            Debug.Log($"Disconnected: {e.ConnectionId}");
            LobbyClosed = true;
            Dirty = true;
        };

        signalR.Connect();
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
                    case UnityViewOptions.PrimaryAxis:
                        Debug.Log(view.Options[key].GetType());
                        view.Options[key] = (long)view.Options[key];
                        break;
                    case UnityViewOptions.PrimaryAxisMaxCount:
                        view.Options[key] = (long?)view.Options[key];
                        break;
                    default:
                        throw new NotImplementedException("Not implemented");
            }
        }

        if (view.UnityObjects?.Value != null)
        {
            List<UnityObject> unityObjects = new List<UnityObject>();
            foreach (object obj in view.UnityObjects.Value)
            {
                JObject jObject = (JObject)obj;
                // Serialization will omit if it is default (i.e. Image)
                switch (jObject["Type"]?.ToObject<UnityObjectType>() ?? UnityObjectType.Image)
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
            signalR.Invoke("ConnectWebLobby", LobbyId + "-" + ClientVersion);
        }
    }
    public void RequestImageFromServer(string imageKey)
    {
        if (Connected)
        {
            signalR.Invoke("ConnectImageRequest", imageKey);
        }
    }

    public void Start()
    {
#if !UNITY_EDITOR && UNITY_WEBGL
        WebGLInput.captureAllKeyboardInput = false;
#endif

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
        if (LobbyClosed)
        {
            LobbyClosed = false;
            ViewManager.Singleton.OnLobbyClose();
        }

        if (!RPCRequestQueue.IsEmpty && RPCRequestQueue.TryDequeue(out var RPCRequest))
        {
            if (RPCRequest.UnityImageList != null)
            {
                foreach (var item in RPCRequest.UnityImageList.ImgList)
                {
                    ImageRepository.AddBase64PngToRepository(item.Key, item.Value);
                }
                /* process image list ViewManager.Singleton.UpdateConfigMetaData(RPCRequest.ConfigurationMetadata); */
            }

            // Just checking this to fill up the user lookup, broadcasting below
            if (RPCRequest.UnityView != null)
            {
                // Maintain a lookup of the latest instance we have seen of a given user. Hack to minimize data sent for UnityUserStatuses
                foreach (UnityUser user in RPCRequest.UnityView.Users)
                {
                    UserLookup[user.Id] = user;
                }
            }

            List<UnityUser> usersAnsweringPrompts = null;
            if (RPCRequest.UnityUserStatus != null)
            {
                usersAnsweringPrompts = new List<UnityUser>();
                foreach (Guid userAnsweringPrompt in RPCRequest.UnityUserStatus.UsersAnsweringPrompts)
                {
                    if (UserLookup.ContainsKey(userAnsweringPrompt))
                    {
                        UserLookup[userAnsweringPrompt].Status = UserStatus.AnsweringPrompts;
                        usersAnsweringPrompts.Add(UserLookup[userAnsweringPrompt]);
                    }
                    else
                    {
                        Debug.LogWarning($"User status guid not found in local user lookup, ignoring user '{userAnsweringPrompt}'");
                    }
                }
            }

            // Update the view after the user statuses so that we can update them together
            if (RPCRequest.UnityView != null)
            {         
                // Don't show usersAnsweringPrompts on reveal, use normal users list per unity view.
                ViewManager.Singleton.SwitchToView(RPCRequest.UnityView?.ScreenId ?? TVScreenId.Unknown, RPCRequest.UnityView, RPCRequest.UnityView.IsRevealing ? null : usersAnsweringPrompts);
            }
            else if (usersAnsweringPrompts != null)
            {
                // Send user status alongside the switch to view when possible (this is so that the change happens after view transition animation!)
                ViewManager.Singleton.UpdateUsersAnsweringPrompts(usersAnsweringPrompts);
            }

            if (RPCRequest.ConfigurationMetadata != null)
            {
                ViewManager.Singleton.UpdateConfigMetaData(RPCRequest.ConfigurationMetadata);
            }
        }
        // If we aren't in the process of a delayed restart and the connection task failed. Begin a delayed restart.
        // plr-old if (!Restarting && (hubConnection?.State != HubConnectionState.Connected || hubTask==null || hubTask.IsFaulted || hubTask.IsCanceled))
        // NOTE: the Connected in the line below only says we EVER connected, not that we are still connected
        if (!Restarting && (Connected || hubTask == null || hubTask.IsFaulted || hubTask.IsCanceled))
        {
            //StartCoroutine(DelayedConnectToHub());
        }
    } 

    private static JsonSerializerSettings SerializerSettings = new JsonSerializerSettings()
    {
        DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate
    };
    private Dictionary<Guid, UnityUser> UserLookup = new Dictionary<Guid, UnityUser>();

    public void OnApplicationQuit()
    {
    }
 
}
