using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameIconHandler : MonoBehaviour
{
    [Serializable]
    public struct GameSpriteDetails
    {
        public GameModeId game;
        public Sprite sprite;
        public Color branding;
    }

    public Image IconHolder;
    public Image BlobBackground;
    public Color DefaultBrandingColor;

    public List<GameSpriteDetails> GameSprites = new List<GameSpriteDetails>();


    // Start is called before the first frame update

    private Dictionary<GameModeId, GameSpriteDetails> GamesToIcons = new Dictionary<GameModeId, GameSpriteDetails>();
    void Start()
    {
        for (int i = 0; i < GameSprites.Count; i++)
        {
            GamesToIcons.Add(GameSprites[i].game, GameSprites[i]);
        }
        ViewManager.Singleton.AddConfigurationListener_GameMode(SetGame);

        SetGame(null);
    }

    public void SetGame(GameModeId? gameMode) 
    {
        if (gameMode != null 
            && GamesToIcons.ContainsKey((GameModeId)gameMode)
            && GamesToIcons[(GameModeId)gameMode].sprite != null) //checking that game has an icon
        {
            IconHolder.sprite = GamesToIcons[(GameModeId)gameMode].sprite;
            BlobBackground.color = GamesToIcons[(GameModeId)gameMode].branding;
            IconHolder.gameObject.SetActive(true);              
        }
        else // if nothing to show for game show scrawlbrawl
        {
            // Don't show game specific icon if no gamemode. Scrawl Brawl logo is always shown
            IconHolder.gameObject.SetActive(false);
            BlobBackground.color = DefaultBrandingColor;
        }
    }
}
