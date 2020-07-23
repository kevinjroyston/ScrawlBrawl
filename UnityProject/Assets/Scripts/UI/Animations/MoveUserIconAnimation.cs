using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.UI;

public class MoveUserIconAnimation : AnimationBase
{
    public Image ScoreProjectilePrefab;
    private int IconOrder;
    private int IconCountTotal;
    private float OrderOffset = 0.4f;
    public override void Awake()
    {
        base.Awake();
        registerOnEnable = false;
    }
    public override List<LTDescr> Animate(GameEvent gameEvent)
    {
        MoveToTargetGameEvent targetGameEvent = (MoveToTargetGameEvent)gameEvent;
        Image createdMarker = Instantiate(ScoreProjectilePrefab, rect);
        createdMarker.color = gameObject.GetComponent<Colorizer>().AssignedColor;
        RectTransform targetScoreRect = targetGameEvent.TargetRect;
        RectTransform markerRect = createdMarker.rectTransform;

        markerRect.localScale = Vector3.zero;
        #region calculating tangent
        float r = 1;
        float x = rect.position.x;
        float y = rect.position.y + 1;
        float tangentY = (2 * r * r * y + Mathf.Sqrt(4 * r * r * r * r * y * y  - 4 * (x * x + y * y) * (r * r * r * r - x * x * r * r))) / (2 * (x * x + y * y));
        float tangentX = Mathf.Sqrt(r * r - tangentY * tangentY);

        Vector3 targetTangent = new Vector3(
            x: tangentX + targetScoreRect.position.x,
            y: tangentY + targetScoreRect.position.y,
            z: targetScoreRect.position.z);
        #endregion

        float speed = 15f;

        List<LTDescr> animations = new List<LTDescr>()
        {
            LeanTween.scale(
                    rectTrans: rect,
                    to: new Vector3(1.1f,1.1f,1.1f),
                    time: 0.2f)
            .SetCallEventOnComplete(new GameEvent(){ eventType = GameEvent.EventEnum.VoteRevealBubbleCreated}),
            LeanTween.scale(
                    rectTrans: rect,
                    to: new Vector3(1f,1f,1f),
                    time: 0.3f)
            .setEaseOutBack()
            .setDelay(0.2f),
            LeanTween.scale(
                rectTrans: markerRect,
                to: Vector3.one,
                time: 0.3f)
            .setEaseOutBack()
            .setDelay(0.2f),
            LeanTweenHelper.Singleton.UIMoveRelative(
                rectTransform: markerRect,
                to: new Vector3(0, 1, 0),
                time: 0.2f)
            .setEaseOutBack()
            .setDelay(0.2f),
            LeanTweenHelper.Singleton.UIMove(
                rectTransform: markerRect,
                to: targetTangent,
                time: Vector2.Distance(markerRect.position, targetTangent) / speed)
            .setDelay(0.4f + IconCountTotal * OrderOffset - Vector2.Distance(markerRect.position, targetTangent) / speed),
            LeanTweenHelper.Singleton.DynamicOrbitAroundPoint(
                rectTransform: markerRect,
                center: targetScoreRect.position,
                radiusValueTween: LeanTween.value(1, 0, 2).setDelay(0.4f + IconCountTotal * OrderOffset),
                radians: 6 * Mathf.PI ,
                time: 6 * Mathf.PI * 1 / speed)
            .setDelay(0.4f + IconCountTotal * OrderOffset),
            LeanTween.scale(
                rectTrans: markerRect,
                to: Vector3.zero,
                time: 1f)
            .setDelay(0.4f + IconCountTotal * OrderOffset +  6 * Mathf.PI * 1 / speed - 1),
            LeanTween.scale(
                    rectTrans: targetScoreRect,
                    to: new Vector3(0.9f,0.9f,0.9f),
                    time: 0.2f)
            .setDelay(0.4f + IconCountTotal * OrderOffset +  6 * Mathf.PI * 1 / speed)
            .SetCallEventOnComplete(new GameEvent(){ eventType = GameEvent.EventEnum.IncreaseScore, id = startEvent.id }),
            LeanTween.scale(
                    rectTrans: targetScoreRect,
                    to: new Vector3(1f,1f,1f),
                    time: 0.3f)
            .setEaseOutBack()
            .setDelay(0.4f + IconCountTotal * OrderOffset +  6 * Mathf.PI * 1 / speed + 0.2f),
        };
        return animations;
    }
    public void AssignUserAndRegister(User relevantUser, int order, int totalNumIcons)
    {
        this.IconOrder = order;
        this.IconCountTotal = totalNumIcons;
        startDelay += order * OrderOffset;
        startEvent.id = relevantUser.UserId.ToString();
        CallRegisterForAnimation();
    }
}
