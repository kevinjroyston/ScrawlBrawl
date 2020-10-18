using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameIconHandler : MonoBehaviour
{
    [Serializable]
    public struct GameSpritePair
    {
        public GameModeId game;
        public Sprite sprite;
    }

    public Image IconHolder = null;
    public Sprite ScrawlBrawlLogo = null;

    public List<GameSpritePair> GameSprites = new List<GameSpritePair>();


    // Start is called before the first frame update

    private Dictionary<GameModeId, Sprite> GamesToIcons = new Dictionary<GameModeId, Sprite>();
    void Start()
    {
        for (int i = 0; i < GameSprites.Count; i++)
        {
            GamesToIcons.Add(GameSprites[i].game, GameSprites[i].sprite);
        }
        ViewManager.Singleton.AddConfigurationListener_GameMode(SetGame);
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
