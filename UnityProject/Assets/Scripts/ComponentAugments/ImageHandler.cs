using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ImageHandler : MonoBehaviour
{
    private List<Image> Images = new List<Image>();
    public GameObject SubImagePrefab;

    public Text ImageId;
    public Text Title;
    public Text Header;

    public Image Background;
    public GameObject SubImageHolder;
    public GameObject ImageIdHolder;

    public GameObject FooterHolder;
    public Text VoteCount;
    public Text DummyVoteCount;

    public UnityImage UnityImage
    {
        set
        {
            Background.preserveAspect = true;

            // Default to invisible background, overridden if subimages present.
            Background.color = new Color(0f, 0f, 0f, 0f);

            if (value?.PngSprites != null)
            {
                for (int i = 0; i < value.PngSprites.Count; i++)
                {
                    if (Images.Count <= i)
                    {
                        var subImage = GameObject.Instantiate(SubImagePrefab);
                        subImage.transform.SetParent(SubImageHolder.transform);
                        subImage.transform.localScale = new Vector3(1f, 1f, 1f);
                        subImage.transform.localPosition = Vector3.zero;
                        Images.Add(subImage.GetComponent<Image>());
                    }
                    Image image = Images[i];
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
}
