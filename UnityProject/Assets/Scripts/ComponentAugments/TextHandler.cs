using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UI.GridLayoutGroup;

public class TextHandler : MonoBehaviour, UnityObjectHandlerInterface
{
    public Text TextId;
    public Text Title;
    public Text Header;
    public Text VerticleScore;

    // Set to be the Image of the first SpriteGrid
    private Image Background = null;
    public GameObject TextIdHolder;
    public GameObject VerticleScoreHolder;

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

    public UnityImage UnityImage
    {
        set
        {
            if (Background != null)
            {
                Background.preserveAspect = true;

                // Default to invisible background, overridden if subimages present.
                Background.color = new Color(0f, 0f, 0f, 0f);
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

            TextId.text = value?._ImageIdentifier ?? string.Empty;
            TextId.enabled = value?._ImageIdentifier != null;
            TextIdHolder.SetActive(!string.IsNullOrWhiteSpace(value?._ImageIdentifier));

            if (value.Options._PrimaryAxis == Axis.Vertical)
            {
                VerticleScore.text = value?._VoteCount.ToString() ?? string.Empty;
                VerticleScore.enabled = value?._VoteCount != null;
                VerticleScoreHolder.SetActive(value?._VoteCount != null);

                VoteCount.enabled = false;
                ScoreHolder.gameObject.SetActive(false);
            }
            else
            {
                VoteCount.text = value?._VoteCount?.ToString() ?? string.Empty;
                VoteCount.enabled = value?._VoteCount != null;
                DummyScore.SetActive(value?._VoteCount != null);
                ScoreHolder.gameObject.SetActive(value?._VoteCount != null);

                VerticleScore.enabled = false;
                VerticleScoreHolder.SetActive(false);
            }
            //bool relevantUsers = value?._RelevantUsers != null && value._RelevantUsers.Any();
            
            // Used by Colorizer to deterministically color UI objects.
            string hashableIdentifier =
                !string.IsNullOrWhiteSpace(value?._ImageIdentifier) ? value._ImageIdentifier
                : !string.IsNullOrWhiteSpace(value?._Title) ? value._Title
                : string.Empty;
            CallColorizers(hashableIdentifier);
        }
    }

    private void CallColorizers(string identifier)
    {
        Colorizer[] colorizers = GetComponentsInChildren<Colorizer>();
        if (colorizers != null)
        {
            foreach (Colorizer colorizer in colorizers)
            {
                colorizer.RefreshColor(identifier);
            }
        }
    }

}
