using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RevealImageAnimation : AnimationBase
{
    public Color backgroundRevealColor = Color.green;
    public Color textRevealColor = Color.green;
    public Image Background;
    public Text Title;

    public override List<LTDescr> Animate(GameEvent gameEvent)
    {
        List<LTDescr> animations = new List<LTDescr>();
        LTDescr scaleUp = TweenAnimator.scale(
                     rectTrans: rect,
                     to: new Vector3(1.1f, 1.1f, 1.1f),
                     time: 0.1f);
        LTDescr scaleDown = TweenAnimator.scale(
                    rectTrans: rect,
                    to: new Vector3(1f, 1f, 1f),
                    time: 0.2f)
            .setEaseOutBack()
            .PlayAfter(scaleUp)
            .SetCallEventOnStart(new GameEvent() { eventType = GameEvent.EventEnum.ShowDeltaScores});
        animations.Add(scaleUp);
        animations.Add(scaleDown);
        if (Background != null)
        {
            LTDescr backgroundChange = TweenAnimator.color(
                rectTrans: Background.rectTransform,
                to: backgroundRevealColor,
                time: 0.1f);
            animations.Add(backgroundChange);
        }
        if (Title != null)
        {
            LTDescr titleChange = TweenAnimator.textColor(
                rectTransform: Title.rectTransform,
                to: textRevealColor,
                time: 0.1f);
            animations.Add(titleChange);
        }

        return animations;
    }
    public void AssignIdAndRegister(string id)
    {
        startEvent.id = id;
        CallRegisterForAnimation();      
    }

}
