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
    private float OrderOffset = 0.1f;
    public override List<LTDescr> Animate(GameEvent gameEvent)
    {
        MoveToTargetGameEvent targetGameEvent = (MoveToTargetGameEvent)gameEvent;
        Image createdMarker = Instantiate(ScoreProjectilePrefab, rect);
        createdMarker.color = gameObject.GetComponent<Colorizer>().AssignedColor;
        RectTransform targetScoreRect = targetGameEvent.TargetRect;
        string targetId = targetGameEvent.TargetUserId;
        RectTransform markerRect = createdMarker.rectTransform;

        Vector2 targetRadiusVector = targetScoreRect.localToWorldMatrix.MultiplyVector(new Vector2(targetScoreRect.rect.width, targetScoreRect.rect.height));
        float targetRadius = Mathf.Min(targetRadiusVector.x, targetRadiusVector.y);
        Vector2 markerRadiusVector = markerRect.localToWorldMatrix.MultiplyVector(new Vector2(markerRect.rect.width, markerRect.rect.height));
        float markerRadius = Mathf.Min(markerRadiusVector.x, markerRadiusVector.y);

        markerRect.localScale = Vector3.zero;
        #region calculating tangent
        float r = targetRadius * 1.1f;
        float x = rect.position.x;
        float y = rect.position.y + 1;
        float tangentY = (2 * r * r * y + Mathf.Sqrt(4 * r * r * r * r * y * y  - 4 * (x * x + y * y) * (r * r * r * r - x * x * r * r))) / (2 * (x * x + y * y));
        float tangentX = Mathf.Sqrt(r * r - tangentY * tangentY);

        Vector3 targetTangent = new Vector3(
            x: tangentX + targetScoreRect.position.x,
            y: tangentY + targetScoreRect.position.y,
            z: targetScoreRect.position.z);
        #endregion

        float speed = Vector2.Distance(markerRect.position, targetTangent) / 0.5f;

        #region tweens
        LTDescr iconScaleDown = LeanTween.scale(
                    rectTrans: rect,
                    to: new Vector3(1.1f, 1.1f, 1.1f),
                    time: 0.2f);
        LTDescr iconScaleUp = LeanTween.scale(
                    rectTrans: rect,
                    to: new Vector3(1f, 1f, 1f),
                    time: 0.3f)
            .setEaseOutBack()
            .PlayAfter(iconScaleDown)
            .SetCallEventOnStart(new GameEvent() { eventType = GameEvent.EventEnum.PlayPop });
        LTDescr markerScaleUp = LeanTween.scale(
                rectTrans: markerRect,
                to: Vector3.one,
                time: 0.3f)
            .setEaseOutBack()
            .PlayAfter(iconScaleDown);
        LTDescr markerMoveUp = LeanTweenHelper.Singleton.UIMoveRelative(
                rectTransform: markerRect,
                to: new Vector3(0, 1, 0),
                time: 0.2f)
            .setEaseOutBack()
            .PlayAfter(iconScaleDown);
        LTDescr markerMoveToTarget = LeanTweenHelper.Singleton.UIMove(
                rectTransform: markerRect,
                to: targetTangent,
                time: 0.5f)
            .SetCallEventOnStart(new GameEvent() { eventType = GameEvent.EventEnum.VoteRevealBubbleMove })
            .PlayAfter(markerMoveUp, IconCountTotal * OrderOffset + 0.0f);
        LTDescr markerScaleUpToTarget = LeanTween.scale(
            rectTrans: markerRect,
            to: new Vector3(targetRadius / markerRadius * 0.5f, targetRadius / markerRadius * 0.5f, targetRadius / markerRadius * 0.5f),
            time: 0.5f)
            .PlayAfter(markerMoveUp, IconCountTotal * OrderOffset + 0.0f);
        LTDescr markerOrbit = LeanTweenHelper.Singleton.DynamicOrbitAroundPoint(
                rectTransform: markerRect,
                center: targetScoreRect.position,
                radiusValueTween: LeanTween.value(targetRadius * 1.1f, targetRadius * 0.6f, 2).PlayAfter(markerMoveToTarget),
                radians: speed * (IconCountTotal * OrderOffset + 0.5f) * targetRadius * 1.1f,
                time: (IconCountTotal * OrderOffset + 0.5f) * targetRadius * 1.1f)
            .PlayAfter(markerMoveToTarget);
        LTDescr markerFall = LeanTweenHelper.Singleton.UIMove(
                rectTransform: markerRect,
                to: targetScoreRect.position,
                time: 0.4f)
            .setEaseOutBounce()
            .PlayAfter(markerOrbit);
        LTDescr targetScaleUp = LeanTween.scale(
                rectTrans: targetScoreRect,
                to: new Vector3(1.1f, 1.1f, 1.1f),
                time: 0.5f)
            .PlayAfter(markerFall, -0.5f);
        LTDescr targetScaleDown = LeanTween.scale(
                    rectTrans: targetScoreRect,
                    to: new Vector3(1f, 1f, 1f),
                    time: 0.3f)
            .setEaseOutBack()
            .PlayAfter(targetScaleUp)
            .AddOnStart(() => Destroy(createdMarker))
            .SetCallEventOnStart(new GameEvent() { eventType = GameEvent.EventEnum.PlayPop })
            .SetCallEventOnStart(new GameEvent() { eventType = GameEvent.EventEnum.IncreaseScore, id = targetId });
        if (IconOrder == IconCountTotal)
        {
            targetScaleDown.SetCallEventOnStart(new GameEvent() { eventType = GameEvent.EventEnum.ShakeRevealImages });
        }


        #endregion

        List<LTDescr> animations = new List<LTDescr>()
        {
            iconScaleDown,
            iconScaleUp,
            markerScaleUp,
            markerMoveUp,
            markerMoveToTarget,
            markerScaleUpToTarget,
            markerOrbit,
            markerFall,
            targetScaleUp,
            targetScaleDown
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
