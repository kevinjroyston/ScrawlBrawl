using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameIconHandler : MonoBehaviour
{
    public Sprite ScrawlBrawlLogo = null;
    public Sprite BodyBuilderLogo = null;
    public Sprite BodySwapLogo = null;
    public Sprite ImposterSyndromeLogo = null;
    public Sprite ChaoticCooperationLogo = null;
    public Sprite MimicLogo = null;
    public Image IconHolder = null;
    // Start is called before the first frame update
    void Start()
    {
        if (IconHolder != null)
        {
            if (ScrawlBrawlLogo != null)
            {
                IconHolder.sprite = ScrawlBrawlLogo;
            }
        }
    }

    public void SetGame() //TODO
    {

    }
}
