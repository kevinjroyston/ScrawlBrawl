using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlurRenderer : MonoBehaviour
{
    public Camera blurCamera;
    public Material blurMaterial;

    void Start()
    {
        if (blurCamera.targetTexture != null)
        {
            blurCamera.targetTexture.Release();
        }
        blurCamera.targetTexture = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32, 1);
        blurMaterial.SetTexture("_RenTex", blurCamera.targetTexture);
    }

}
