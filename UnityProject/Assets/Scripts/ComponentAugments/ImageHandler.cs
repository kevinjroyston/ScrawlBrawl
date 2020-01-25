using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ImageHandler : MonoBehaviour
{
    public Image Image;
    public Image Background;
    public GameObject VoteCountHolder;
    public Text VoteCount;
    public Text Title;
    public Text Header;
    public Text Footer;
    public UnityImage UnityImage
    {
        set
        {
            // TODO: multi-sprites.
            Sprite sprite = value?.PngSprites?.FirstOrDefault();

            Background.preserveAspect = true;
            Image.preserveAspect = true;

            Image.sprite = sprite;
            Image.color = sprite == null ? new Color(0f, 0f, 0f, 0f) : Color.white;

            // TODO: find better option than creating new sprite for getting aspect ratio correct.
            Background.sprite = Sprite.Create(new Texture2D((int)sprite.rect.width, (int)sprite.rect.height), sprite.rect, sprite.pivot, 10f, 0, SpriteMeshType.FullRect);
            Background.color = sprite == null ? new Color(0f, 0f, 0f, 0f) : value?._BackgroundColor?.ToColor() ?? Color.white;

            Title.text = value?._Title ?? string.Empty;
            Title.enabled = value?._Title != null;
            Title.gameObject.SetActive(!string.IsNullOrWhiteSpace(value?._Title));

            Header.text = value?._Header ?? string.Empty;
            Header.enabled = value?._Header != null;
            Header.gameObject.SetActive(!string.IsNullOrWhiteSpace(value?._Header));

            Footer.text = value?._Footer ?? string.Empty;
            Footer.enabled = value?._Footer != null;
            Footer.gameObject.SetActive(!string.IsNullOrWhiteSpace(value?._Footer));

            VoteCount.text = value?._VoteCount?.ToString() ?? string.Empty;
            VoteCountHolder.SetActive(value?._VoteCount != null);
            //VoteCount.enabled = value?._VoteCount != null;
            VoteCount.gameObject.SetActive(value?._VoteCount != null);

            // TODO: relevant users list.
        }
    }
}
