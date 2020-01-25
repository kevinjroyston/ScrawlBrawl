using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public static class TextureUtilities
{
    public static Sprite LoadTextureFromBase64(string rawPng)
    {
        var base64Data = Regex.Match(rawPng, @"data:image/(?<type>.+?),(?<data>.+)").Groups["data"].Value;
        byte[] pictureBytes = System.Convert.FromBase64String(base64Data);

        Texture2D tex = new Texture2D(1, 1);
        tex.LoadImage(pictureBytes);

        Sprite sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 1f, 0, SpriteMeshType.FullRect);

        return sprite;
    }
}
