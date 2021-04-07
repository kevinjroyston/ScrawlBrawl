using Assets.Scripts.Networking.DataModels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnterLobbyId : ITVView
{
    public Text UserInput;
    public TestClient TestClient;
    public void Awake()
    {
#if UNITY_EDITOR
        ViewManager.Singleton.RegisterAsDefaultTVView(this);
#else
        gameObject.SetActive(false);
#endif
    }

    public void ButtonPressed()
    {
        TestClient.ConnectToLobby(UserInput.text.Trim());
    }
}
