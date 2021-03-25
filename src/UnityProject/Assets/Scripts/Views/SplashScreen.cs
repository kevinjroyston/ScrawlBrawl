using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplashScreen : ITVView
{
    public void Awake()
    {
#if !UNITY_EDITOR
        ViewManager.Singleton.RegisterAsDefaultTVView(this);
#else
        gameObject.SetActive(false);
#endif
    }
}
