using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoveUserIconAnimation : AnimationBase
{
    public Image ScoreMarkerPrefab;

    public void Awake()
    {
        startEvent.id = gameObject.GetComponent<RelevantUserPopulator>().IconUser.UserId;
    }
    public override List<LTDescr> Animate(GameEvent gameEvent)
    {
        MoveToTargetGameEvent targetGameEvent = (MoveToTargetGameEvent)gameEvent;
        Image createdMarker = Instantiate(ScoreMarkerPrefab, this.gameObject.transform);
        RectTransform markerRect = createdMarker.rectTransform;
        RectTransform targetScoreRect = targetGameEvent.TargetRect;
        

        List<LTDescr> animations = new List<LTDescr>()
        {
            LeanTween.moveY(
                rectTrans: markerRect,
                to: markerRect.rect.y + 10,
                time: 0.7f).setEaseOutCirc(),
            LeanTween.move(
                rectTrans: markerRect,
                to: targetScoreRect.rect.position,
                time: 1.2f).setEaseOutCirc(),
            LeanTween.scale(
                    rectTrans: targetScoreRect,
                    to: new Vector3(0.9f,0.9f,0.9f),
                    time: 0.2f),
            LeanTween.scale(
                    rectTrans: targetScoreRect,
                    to: new Vector3(1f,1f,1f),
                    time: 0.5f).setEaseOutBack(),
        };
        AnimationHelpers.MakeAnimationsStaged(animations);
        return animations;
    }
   
}
