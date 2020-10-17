 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameIconHandler : MonoBehaviour
{
    public Image IconHolder = null;
    public Sprite ScrawlBrawlLogo = null;

    public List<GameModeId> games = new List<GameModeId>();
    public List<Sprite> sprites = new List<Sprite>();

    // Start is called before the first frame update

    private Dictionary<GameModeId, Sprite> GamesToIcons = new Dictionary<GameModeId, Sprite>();
    void Start()
    {
        if (games.Count != sprites.Count)
        {
            throw new System.Exception("Games and Sprites must be the same length (items are one to one by index)");
        }
        for (int i = 0; i < games.Count; i++)
        {
            GamesToIcons.Add(games[i], sprites[i]);
        }
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
