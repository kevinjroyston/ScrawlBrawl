using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ImageHandler : MonoBehaviour
{
    private List<Image> Images = new List<Image>();
    private List<GameObject> ImageGrids = new List<GameObject>();
    public GameObject SpriteGridPrefab;
    public GameObject SpriteObjectPrefab;

    public Text ImageId;
    public Text Title;
    public Text Header;

    public Image Background;
    public GameObject SpriteZone;
    public GameObject ImageIdHolder;

    public GameObject FooterHolder;
    public Text VoteCount;
    public Text DummyVoteCount;

    private List<Action<float>> AspectRatioListeners = new List<Action<float>>();

    public UnityImage UnityImage
    {
        set
        {
            Background.preserveAspect = true;

            // Default to invisible background, overridden if subimages present.
            Background.color = new Color(0f, 0f, 0f, 0f);

            if (value?.PngSprites != null)
            {
                float aspectRatio = 1f;
                if (value.PngSprites.Count > 0)
                {
                    aspectRatio = value.PngSprites[0].textureRect.width / value.PngSprites[0].textureRect.height;
                }
                int gridCapacity = value._SpriteGridWidth.GetValueOrDefault(1) * value._SpriteGridHeight.GetValueOrDefault(1);
                // This instantiates 1 extra grid in some scenarios but that doesn't matter.
                int requiredGridCount = value.PngSprites.Count / gridCapacity;
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
                    autoScaleScript.fixedDimensions = new Vector2(value._SpriteGridWidth.GetValueOrDefault(1), value._SpriteGridHeight.GetValueOrDefault(1));
                    if (i == 0)
                    {
                        CallAspectRatioListeners(((float)value._SpriteGridWidth.GetValueOrDefault(1)) / ((float)value._SpriteGridHeight.GetValueOrDefault(1)) * aspectRatio);
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
                    if (i == 0)
                    {
                        Background.sprite = Sprite.Create(new Texture2D((int)sprite.rect.width, (int)sprite.rect.height), sprite.rect, sprite.pivot, 10f, 0, SpriteMeshType.FullRect);
                        Background.color = value?._BackgroundColor?.ToColor() ?? Color.white;
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

            // TODO: Footer
            //Footer.text = value?._Footer ?? string.Empty;
            //Footer.enabled = value?._Footer != null;
            //Footer.gameObject.SetActive(!string.IsNullOrWhiteSpace(value?._Footer));

            ImageId.text = value?._ImageIdentifier ?? string.Empty;
            ImageId.enabled = value?._ImageIdentifier != null;
            ImageIdHolder.SetActive(!string.IsNullOrWhiteSpace(value?._ImageIdentifier));

            // Janky implementation of votecount/footer. probably needs some revisiting. Good luck.
            VoteCount.text = value?._VoteCount?.ToString() ?? string.Empty;
            VoteCount.enabled = value?._VoteCount != null;
            DummyVoteCount.text = value?._VoteCount?.ToString() ?? string.Empty;
            DummyVoteCount.enabled = value?._VoteCount != null;
            DummyVoteCount.gameObject.SetActive(value?._VoteCount != null);
            FooterHolder.gameObject.SetActive(value?._VoteCount != null);

            //VoteCount.enabled = value?._VoteCount != null;
            //VoteCount.gameObject.SetActive(value?._VoteCount != null);

            // TODO: relevant users list.
        }
    }

    private float lastUsedAspectRatio = 1f;
    public void RegisterAspectRatioListener(Action<float> listener)
    {
        AspectRatioListeners.Add(listener);
        listener.Invoke(lastUsedAspectRatio);
    }
    const float minAspectRatio = .3f;
    const float maxAspectRatio = 3.0f;
    private void CallAspectRatioListeners(float value)
    {
        if (value < minAspectRatio)
        {
            value = minAspectRatio;
        }
        else if (value > maxAspectRatio)
        {
            value = maxAspectRatio;
        }
        lastUsedAspectRatio = value;
        foreach (var func in AspectRatioListeners)
        {
            // Tells the listeners what size this layout group would ideally like to be
            func.Invoke(value);
        }
    }

}
