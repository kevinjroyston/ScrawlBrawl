using Assets.Scripts.Views.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Networking.DataModels;
using Assets.Scripts.Views.DataModels;
using static TypeEnums;
using Assets.Scripts.ComponentAugments;
using static PrefabLookup;

public class SpriteHandler : MonoBehaviour, HandlerInterface, CustomAspectRatio
{
    public Image BackgroundImage;
    public Image BlurMask;
    public List<HandlerId> HandlerIds => HandlerType.Sprite.ToHandlerIdList();
    public HandlerScope Scope => HandlerScope.UnityObject;


    private List<Image> Images { get; set; } = new List<Image>();
    private List<GameObject> ImageGrids { get; set; } = new List<GameObject>();
    private int SpriteGridWidth { get; set; }
    private int SpriteGridHeight { get; set; }
    private List<Sprite> Sprites { get; set; }
    private Color BackgroundColor { get; set; }

    private Action<float> AspectRatioListener { get; set; }

    public void OnDestroy()
    {
        BlurController.Singleton.blurMasks.Remove(BlurMask);
    }

    public void RegisterAspectRatioListener(Action<float> listener)
    {
        AspectRatioListener = listener;
    }

    public void UpdateValue(SpriteHolder spriteHolder)  
    {
        BlurController.Singleton.blurMasks.Add(BlurMask);
        BlurMask.enabled = false;

        BackgroundColor = spriteHolder.BackgroundColor?.Value?.ToColor() ?? Color.white;
        BackgroundImage.color = BackgroundColor;
        SpriteGridWidth = spriteHolder.SpriteGridWidth ?? 1;
        SpriteGridHeight = spriteHolder.SpriteGridHeight ?? 1;
        Sprites = spriteHolder.Sprites?.ToList() ?? new List<Sprite>();

        int gridCapacity = SpriteGridWidth * SpriteGridHeight;
        int requiredGridCount = Sprites.Count / gridCapacity;


        if (Images.Count > 0)
        {
            for (int i = (Sprites.Count); i < Images.Count; i++)
            {
                Destroy(Images[i].gameObject);
            }
            Images = Images.GetRange(0, Math.Min(Images.Count, Sprites.Count));
        }

        if (ImageGrids.Count > 0)
        {
            for (int i = requiredGridCount; i < ImageGrids.Count; i++)
            {
                Destroy(ImageGrids[i]);
            }
            ImageGrids = ImageGrids.GetRange(0, Math.Min(ImageGrids.Count, requiredGridCount));
        }

        for (int i = 0; i <= requiredGridCount; i++)
        {
            if (ImageGrids.Count <= i)
            {
                var grid = GameObject.Instantiate(PrefabLookup.Singleton.Mapping[PrefabType.SpriteGrid]);
                grid.transform.SetParent(transform);
                grid.transform.localScale = new Vector3(1f, 1f, 1f);
                grid.transform.localPosition = Vector3.zero;
                ImageGrids.Add(grid);
            }

            var autoScaleScript = ImageGrids[i].GetComponent<ImageGridHandler>();
            autoScaleScript.aspectRatio = Sprites[0].rect.width * 1.0f / Sprites[0].rect.height;
            autoScaleScript.fixedDimensions = new Vector2(SpriteGridWidth, SpriteGridHeight);
        }

        AspectRatioListener?.Invoke(Sprites[0].rect.width * 1.0f / Sprites[0].rect.height * SpriteGridWidth * 1.0f / SpriteGridHeight);

        for (int i = 0; i < Sprites.Count; i++)
        {
            if (Images.Count <= i)
            {
                var subImage = GameObject.Instantiate(PrefabLookup.Singleton.Mapping[PrefabType.SpriteObject]);
                Images.Add(subImage.GetComponent<Image>());
            }

            Image image = Images[i];
            image.transform.SetParent(ImageGrids[i / gridCapacity].transform);
            image.transform.localScale = new Vector3(1f, 1f, 1f);
            image.transform.localPosition = Vector3.zero;

            Sprite sprite = Sprites[i];


            image.preserveAspect = true;
            image.sprite = Sprites[i];
            image.color = Sprites[i] == null ? new Color(0f, 0f, 0f, 0f) : Color.white;
        }

        // Make any extra sub images invisible
        for (int i = Sprites.Count(); i < Images.Count; i++)
        {
            Images[i].color = new Color(0f, 0f, 0f, 0f);
        }
        
    }

    public void UpdateValue(List<object> objects)
    {
        this.UpdateValue((SpriteHolder) objects[0]);
    }
}
