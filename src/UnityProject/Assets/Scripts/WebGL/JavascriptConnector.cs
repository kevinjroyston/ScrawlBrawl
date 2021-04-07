using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JavascriptConnector : MonoBehaviour
{
    public TestClient TestClient;
    public void ConnectToLobby(string lobbyId)
    {
        UnityEngine.WebGLInput.captureAllKeyboardInput = false;
        TestClient.ConnectToLobby(lobbyId);
    }

    public void Start()
    {
#if !UNITY_EDITOR && UNITY_WEBGL
        WebGLInput.captureAllKeyboardInput = false;
#endif
    }
}
