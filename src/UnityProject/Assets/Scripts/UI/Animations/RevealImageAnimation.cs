using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RevealImageAnimation : AnimationBase
{
    // TODO - this should really be a list of Image / Color pairs. Maybe with other animation settings as well (looping/shimmer/etc) if relevant
    public Color imageBackgroundRevealColor = Color.green;
    public Color cardBackgroundRevealColor = Color.green;
    public Color textRevealColor = Color.green;
    public Image ImageBackground;
    public Image CardBackground;
    public Text Title;

    public override List<LTDescr> Animate(GameEvent gameEvent)
    {
        List<LTDescr> animations = new List<LTDescr>();
        LTDescr scaleUp = LeanTween.scale(
                     rectTrans: rect,
                     to: new Vector3(1.1f, 1.1f, 1.1f),
                     time: 0.1f);
        LTDescr scaleDown = LeanTween.scale(
                    rectTrans: rect,
                    to: new Vector3(1f, 1f, 1f),
                    time: 0.2f)
            .setEaseOutBack()
            .PlayAfter(scaleUp)
            .SetCallEventOnComplete(new GameEvent() { eventType = GameEvent.EventEnum.ShowDeltaScores});
        animations.Add(scaleUp);
        animations.Add(scaleDown);
        if (ImageBackground != null)
        {
            LTDescr backgroundChange = LeanTween.color(
                rectTrans: ImageBackground.rectTransform,
                to: imageBackgroundRevealColor,
                time: 0.1f);
            animations.Add(backgroundChange);
        }
        if (CardBackground != null)
        {
            LTDescr backgroundChange = LeanTween.color(
                rectTrans: CardBackground.rectTransform,
                to: cardBackgroundRevealColor,
                time: 0.1f);
            animations.Add(backgroundChange);
        }
        if (Title != null)
        {
            LTDescr titleChange = LeanTween.textColor(
                rectTransform: Title.rectTransform,
                to: textRevealColor,
                time: 0.1f);
            animations.Add(titleChange);
        }

        return animations;
    }
    public void RegisterForRevealEvent()
    {
        startEvent.eventType = GameEvent.EventEnum.RevealImages;
        CallRegisterForAnimation();      
    }

}
