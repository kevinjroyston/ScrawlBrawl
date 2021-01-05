using Assets.Scripts.ComponentAugments;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixedAspectRatio : MonoBehaviour, CustomAspectRatio
{
    public float AspectRatio;

    public void RegisterAspectRatioListener(Action<float> listener)
    {
        listener(AspectRatio);
    }
}
