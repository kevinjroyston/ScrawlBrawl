using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FinalScoreAnimation : AnimationBase
{
    private int Rank;
    public List<ParticleSystem> Top3Celebrations = new List<ParticleSystem>();
    public List<ParticleSystem> Top3Bursts = new List<ParticleSystem>();
    public Color goldColor;
    public Color goldBackgroundColor;
    public Color silverColor;
    public Color silverBackgroundColor;
    public Color bronzeColor;
    public Color bronzeBackgroundColor;

    public Image holderImage;
    public Image backgroundImage;

    // Stores length of each ranks reveal in seconds
    private readonly List<float> rankRevealLengths = new List<float>{ 
        1.5f,
        1.5f,
        1.25f,
        1.0f,
        1.0f
    };

    private readonly List<GameEvent.EventEnum> rankRevealSounds = new List<GameEvent.EventEnum>{
        GameEvent.EventEnum.PlayCelebrateA,
        GameEvent.EventEnum.PlayCelebrateG,
        GameEvent.EventEnum.PlayCelebrateF,
        GameEvent.EventEnum.PlayBrassHit,
        GameEvent.EventEnum.PlayBrassHit
    };

    private readonly List<float> rankHorizontalOffset = new List<float>
    {
        0f,
        60f,
        80f,
        60f,
        0f
    };

    private readonly List<float> rankImageOvershootScales = new List<float>
    {
        1.4f,
        1.3f,
        1.2f,
        1.0f,
        1.0f
    };

    private readonly List<float> rankImageFinalScales = new List<float>
    {
        1.1f,
        1.0f,
        0.9f,
        0.8f,
        0.8f
    };

    private readonly List<float> rankShakeMagnintudes = new List<float>
    {
        0.03f,
        0.02f,
        0.01f,
        0f,
        0f
    };


    public override List<LTDescr> Animate(GameEvent gameEvent)
    {
        Rank = transform.GetSiblingIndex();
        rect.localScale = new Vector3(0f, 0f, 0f);
        rect.localPosition += new Vector3(rankHorizontalOffset[Rank], 0f, 0f);
        switch (Rank)
        {
            case 0:
                holderImage.color = goldColor;
                backgroundImage.color = goldBackgroundColor;
                break;
            case 1:
                holderImage.color = silverColor;
                backgroundImage.color = silverBackgroundColor;
                break;
            case 2:
                holderImage.color = bronzeColor;
                backgroundImage.color = bronzeBackgroundColor;
                break;
            default:
                break;
        }

        float delay = 0f;
        for (int i = Rank; i < 5; i++)
        {
            delay += rankRevealLengths[i];
        }

        List<LTDescr> animations = new List<LTDescr>();

        LTDescr scaleUp = LeanTween.scale(
                     rectTrans: rect,
                     to: new Vector3(rankImageOvershootScales[Rank], rankImageOvershootScales[Rank], rankImageOvershootScales[Rank]),
                     time: 0.1f)
            .setDelay(delay)
            .setOnStart(() => {
                if (Rank < 3)
                {
                    Top3Celebrations[Rank].Play();
                    Top3Bursts[Rank].Play();
                }
                return;
            })
            .SetCallEventOnComplete(new GameEvent { eventType = rankRevealSounds[Rank] });


        LTDescr scaleDown = LeanTween.scale(
                    rectTrans: rect,
                    to: new Vector3(rankImageFinalScales[Rank], rankImageFinalScales[Rank], rankImageFinalScales[Rank]),
                    time: 0.2f)
            .setEaseOutBack()
            .PlayAfter(scaleUp)
            .SetCallEventOnComplete(new GameEvent() { eventType = GameEvent.EventEnum.ShowDeltaScores });


        if (Rank < 3)
        {
               LTDescr shake = LeanTweenHelper.Singleton.UIShake(
               rectTransform: rect,
               magnitude: rankShakeMagnintudes[Rank],
               frequency: 1f,
               rateOfChange: 0.0001f,
               time: 1000000f);
                animations.Add(shake);
        }
        
        animations.Add(scaleUp);
        animations.Add(scaleDown);

        return animations;
    }

}
