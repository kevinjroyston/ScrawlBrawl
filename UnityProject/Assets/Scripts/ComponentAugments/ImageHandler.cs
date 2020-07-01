using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ImageHandler : MonoBehaviour, UnityObjectHandlerInterface
{
    private List<Image> Images = new List<Image>();
    private List<GameObject> ImageGrids = new List<GameObject>();
    public GameObject SpriteGridPrefab;
    public GameObject SpriteObjectPrefab;

    public Text ImageId;
    public Text Title;
    public Text Header;

    // Set to be the Image of the first SpriteGrid
    private Image Background = null;
    public GameObject SpriteZone;
    public GameObject ImageIdHolder;

    public Text VoteCount;
    public Text Footer;

    /// <summary>
    /// Score overlaps half on half off the card. This gameObject is the part hanging off the card.
    /// </summary>
    public GameObject ScoreHolder;
    /// <summary>
    /// Score overlaps half on half off the card. This dummy is the bottom part of the card reserved for footer.
    /// </summary>
    public GameObject DummyScore;

    /// <summary>
    /// first float is inner(image grid) aspect ratio, second float is outer(entire card w/o padding) aspect ratio for perfect fit UI
    /// </summary>
    private List<Action<float, float>> AspectRatioListeners = new List<Action<float, float>>();

    public UnityImage UnityImage
    {
        set
        {
            int gridCapacity = value._SpriteGridWidth.GetValueOrDefault(1) * value._SpriteGridHeight.GetValueOrDefault(1);
            // This instantiates 1 extra grid in some scenarios but that doesn't matter.
            int requiredGridCount = (value?.PngSprites?.Count ?? 0) / gridCapacity;

            if (Images.Count > 0)
            {
                for (int i = (value?.PngSprites?.Count ?? 0); i < Images.Count ; i++)
                {
                    Destroy(Images[i].gameObject);
                }
                Images = Images.GetRange(0, Math.Min(Images.Count, (value?.PngSprites?.Count ?? 0)));
            }

            if (ImageGrids.Count > 0)
            {
                for (int i = requiredGridCount; i < ImageGrids.Count; i++)
                {
                    Destroy(ImageGrids[i]);
                }
                ImageGrids = ImageGrids.GetRange(0, Math.Min(ImageGrids.Count, requiredGridCount));
            }

            if (Background!= null)
            {
                Background.preserveAspect = true;

                // Default to invisible background, overridden if subimages present.
                Background.color = new Color(0f, 0f, 0f, 0f);
            }

            int gridColCount = value._SpriteGridWidth.GetValueOrDefault(1);
            int gridRowCount = value._SpriteGridHeight.GetValueOrDefault(1);
            float aspectRatio = 1f;
            if (value?.PngSprites != null)
            {
                if (value.PngSprites.Count > 0)
                {
                    aspectRatio = value.PngSprites[0].textureRect.width / value.PngSprites[0].textureRect.height;
                }
                for (int i = 0; i <= requiredGridCount; i++)
                {
                    if(ImageGrids.Count <= i)
                    {
                        var grid = GameObject.Instantiate(SpriteGridPrefab);
                        grid.transform.SetParent(SpriteZone.transform);
                        grid.transform.localScale = new Vector3(1f, 1f, 1f);
                        grid.transform.localPosition = Vector3.zero;
                        ImageGrids.Add(grid);
                    }

                    var autoScaleScript = ImageGrids[i].GetComponent<FixedDimensionAutoScaleGridLayoutGroup>();
                    autoScaleScript.aspectRatio = aspectRatio;
                    autoScaleScript.fixedDimensions = new Vector2(gridColCount, gridRowCount);
                    if (i == 0)
                    {
                        Background = ImageGrids[0].GetComponent<Image>();
                    }
                }

                for (int i = 0; i < value.PngSprites.Count; i++)
                {
                    if (Images.Count <= i)
                    {
                        var subImage = GameObject.Instantiate(SpriteObjectPrefab);
                        Images.Add(subImage.GetComponent<Image>());
                    }

                    Image image = Images[i];
                    image.transform.SetParent(ImageGrids[i / gridCapacity].transform);
                    image.transform.localScale = new Vector3(1f, 1f, 1f);
                    image.transform.localPosition = Vector3.zero;

                    Sprite sprite = value.PngSprites[i];

                    // Set background if we have any sub images.
                    if (i == 0 && Background != null)
                    {
                        Background.sprite = Sprite.Create(
                            new Texture2D(
                                (int)(sprite.rect.width * gridColCount),
                                (int)(sprite.rect.height * gridRowCount)),
                            new Rect(sprite.rect.x, sprite.rect.y, sprite.rect.width * gridColCount, sprite.rect.height * gridRowCount),
                            sprite.pivot,
                            10f,
                            0,
                            SpriteMeshType.FullRect);
                        Background.color = value?._BackgroundColor?.ToColor() ?? Color.white;
                        Background.preserveAspect = true;
                    }

                    image.preserveAspect = true;
                    image.sprite = value.PngSprites[i];
                    image.color = value.PngSprites[i] == null ? new Color(0f, 0f, 0f, 0f) : Color.white;
                }
            }

            // Make any extra sub images invisible
            for (int i = value?.PngSprites?.Count ?? 0; i < Images.Count; i++)
            {
                Images[i].color = new Color(0f, 0f, 0f, 0f);
            }

            Title.text = value?._Title ?? string.Empty;
            Title.enabled = value?._Title != null;
            Title.gameObject.SetActive(!string.IsNullOrWhiteSpace(value?._Title));

            Header.text = value?._Header ?? string.Empty;
            Header.enabled = value?._Header != null;
            Header.gameObject.SetActive(!string.IsNullOrWhiteSpace(value?._Header));

            Footer.text = value?._Footer ?? string.Empty;
            Footer.enabled = value?._Footer != null;
            Footer.gameObject.SetActive(!string.IsNullOrWhiteSpace(value?._Footer));

            ImageId.text = value?._ImageIdentifier ?? string.Empty;
            ImageId.enabled = value?._ImageIdentifier != null;
            ImageIdHolder.SetActive(!string.IsNullOrWhiteSpace(value?._ImageIdentifier));

            //bool relevantUsers = value?._RelevantUsers != null && value._RelevantUsers.Any();
            VoteCount.text = value?._VoteCount?.ToString() ?? string.Empty;
            VoteCount.enabled = value?._VoteCount != null;
            DummyScore.SetActive(value?._VoteCount != null);
            ScoreHolder.gameObject.SetActive(value?._VoteCount != null);


            // Used by Colorizer to deterministically color UI objects.
            string hashableIdentifier =
                !string.IsNullOrWhiteSpace(value?._ImageIdentifier) ? value._ImageIdentifier
                : !string.IsNullOrWhiteSpace(value?._Title) ? value._Title
                : string.Empty;
            CallColorizers(hashableIdentifier);

            /// Aspect ratio shenanigans
            float innerAspectRatio = ((float)gridColCount) / ((float)gridRowCount) * aspectRatio;
            // Code doesnt work if there are no inner images, in this case just default to A.R. 2.0
            float outerAspectRatio = 2f;

            if (GetFlexibleHeightOrDefault(SpriteZone) > 0.01f)
            {
                outerAspectRatio =
                     innerAspectRatio
                     / (GetFlexibleHeightOrDefault(Title)
                         + GetFlexibleHeightOrDefault(Header)
                         + GetFlexibleHeightOrDefault(ScoreHolder)
                         + GetFlexibleHeightOrDefault(Footer)
                         + GetFlexibleHeightOrDefault(DummyScore)
                         + GetFlexibleHeightOrDefault(SpriteZone))
                     * GetFlexibleHeightOrDefault(SpriteZone);
            }
            CallAspectRatioListeners(innerAspectRatio, outerAspectRatio);
        }
    }

    private float GetFlexibleHeightOrDefault(Component comp, float defaultValue = 0f)
    {
        return GetFlexibleHeightOrDefault(comp?.gameObject, defaultValue);
    }
    private float GetFlexibleHeightOrDefault(GameObject obj, float defaultValue = 0f)
    {
        float? flexHeight = obj?.transform?.GetComponent<LayoutElement>()?.flexibleHeight;
        return ((obj?.activeInHierarchy == true) && flexHeight.HasValue) ? flexHeight.Value : defaultValue;
    }

    private float lastUsedInnerAspectRatio = 1f;
    private float lastUsedOuterAspectRatio = 1f;
    public void RegisterAspectRatioListener(Action<float, float> listener)
    {
        AspectRatioListeners.Add(listener);
        listener.Invoke(lastUsedInnerAspectRatio, lastUsedOuterAspectRatio);
    }

    public float minAspectRatio = .3f;
    public float maxAspectRatio = 3.3f;
    private void CallAspectRatioListeners(float innerValue, float outerValue)
    {
        if (innerValue < minAspectRatio)
        {
            innerValue = minAspectRatio;
        }
        else if (innerValue > maxAspectRatio)
        {
            innerValue = maxAspectRatio;
        }
        if (outerValue < minAspectRatio)
        {
            outerValue = minAspectRatio;
        }
        else if (outerValue > maxAspectRatio)
        {
            outerValue = maxAspectRatio;
        }
        lastUsedInnerAspectRatio = innerValue;
        lastUsedOuterAspectRatio = outerValue;
        foreach (var func in AspectRatioListeners)
        {
            // Tells the listeners what size this layout group would ideally like to be
            func.Invoke(innerValue, outerValue);
        }
    }

    private void CallColorizers(string identifier)
    {
        Colorizer[] colorizers = GetComponentsInChildren<Colorizer>();
        if (colorizers!= null)
        {
            foreach(Colorizer colorizer in colorizers)
            {
                colorizer.RefreshColor(identifier);
            }
        }
    }

}
