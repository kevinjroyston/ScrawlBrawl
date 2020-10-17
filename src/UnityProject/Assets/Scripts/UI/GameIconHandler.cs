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

    private Dictionary<GameModeId, Sprite> GamesToIcons = new Dictionary<GameModeId, Sprite>();
    void Start()
    {
        print("started");
        #region adding to dictionary
        GamesToIcons.Add(GameModeId.BodyBuilder, BodyBuilderLogo);
        GamesToIcons.Add(GameModeId.BodySwap, BodySwapLogo);
        GamesToIcons.Add(GameModeId.Imposter, ImposterSyndromeLogo);
        GamesToIcons.Add(GameModeId.ChaoticCoop, ChaoticCooperationLogo);
        GamesToIcons.Add(GameModeId.Mimic, MimicLogo);
        #endregion
        ViewManager.Singleton.RegisterIcon(SetGame);
    }

    public void SetGame(GameModeId? gameMode) 
    {
        if (IconHolder != null)
        {
            if (gameMode != null 
                && GamesToIcons.ContainsKey((GameModeId)gameMode)
                && GamesToIcons[(GameModeId)gameMode] != null) //checking that game has an icon
            {
                IconHolder.sprite = GamesToIcons[(GameModeId)gameMode];
               
            }
            else // if nothing to show for game show scrawlbrawl
            {
                if (ScrawlBrawlLogo != null)
                {
                    IconHolder.sprite = ScrawlBrawlLogo;
                }
            }
        }
    }
}
