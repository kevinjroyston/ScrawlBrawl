using Assets.Scripts.Networking.DataModels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Linq;
using Assets.Scripts.Networking.DataModels.Enums;
using Assets.Scripts.Networking.DataModels.UnityObjects;

public class ImageRepository : MonoBehaviour
{
    public static ImageRepository Singleton;
    public ImageRepository()
    {
        Singleton = this;
    }

    private static Dictionary<string, Sprite> imageRepo = new Dictionary<string, Sprite>();

    public static void AddBase64PngToRepository(string imageId, string rawPng)
    {
        /* rawPng = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAUAAAAFCAYAAACNbyblAAAAHElEQVQI12P4//8/w38GIAXDIBKE0DHxgljNBAAO9TXL0Y4OHwAAAABJRU5ErkJggg==";*/
        var base64Data = Regex.Match(rawPng, @"data:image/(?<type>.+?),(?<data>.+)").Groups["data"].Value;
        byte[] pictureBytes = System.Convert.FromBase64String(base64Data);

        Texture2D tex = new Texture2D(1, 1);
        tex.LoadImage(pictureBytes);

        Sprite sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 1f, 0, SpriteMeshType.FullRect);
        imageRepo[imageId] = sprite;
    }

    public static bool FindImageFromKey(string key, out Sprite sprite)
    {
        return ImageRepository.imageRepo.TryGetValue(key, out sprite);
    }

    public static string PLRTestProcessImage(string rawPng)
    {
        string id = imageRepo.Count.ToString();
        AddBase64PngToRepository(id, rawPng);
        return id;
    }

}
