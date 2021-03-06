﻿using Assets.Scripts.Networking.DataModels;
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
        ViewManager.Singleton.RegisterAsDefaultTVView(this);
    }

    public void ButtonPressed()
    {
        TestClient.ConnectToLobby(UserInput.text.Trim());
    }
}
