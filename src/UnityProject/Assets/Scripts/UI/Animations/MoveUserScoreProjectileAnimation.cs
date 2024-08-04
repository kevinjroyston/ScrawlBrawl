using Assets.Scripts.Networking.DataModels;
using Assets.Scripts.Views.ViewComponentHandlers;
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.UI;

public class MoveUserScoreProjectileAnimation : AnimationBase
{
    public Image ScoreProjectilePrefab;
    private int IconOrder;
    private int IconCountTotal;
    private float OrderOffset = 0.1f;
    public override List<LTDescr> Animate(GameEvent gameEvent)
    {
        MoveToTargetGameEvent targetGameEvent = (MoveToTargetGameEvent)gameEvent;
        Image createdMarker = Instantiate(ScoreProjectilePrefab, rect);
        createdMarker.color = gameObject.GetComponent<ColorizerHandler>().AssignedColor;
        RectTransform targetScoreRect = targetGameEvent.TargetRect;
        RectTransform markerRect = createdMarker.rectTransform;

        Vector2 iconRadiusVector = rect.localToWorldMatrix.MultiplyVector(new Vector2(rect.rect.width, rect.rect.height));
        float iconRadius = Mathf.Min(iconRadiusVector.x, iconRadiusVector.y) / 2;
        Vector2 targetDiameterVector = targetScoreRect.localToWorldMatrix.MultiplyVector(new Vector2(targetScoreRect.rect.width, targetScoreRect.rect.height));
        float targetDiameter = Mathf.Min(targetDiameterVector.x, targetDiameterVector.y);
        Vector2 markerDiameterVector = markerRect.localToWorldMatrix.MultiplyVector(new Vector2(markerRect.rect.width, markerRect.rect.height));
        float markerDiameter = Mathf.Min(markerDiameterVector.x, markerDiameterVector.y);

        markerRect.localScale = Vector3.zero;
        float x1 = rect.position.x;
        float y1 = rect.position.y;

        float cx = targetScoreRect.position.x;
        float cy = targetScoreRect.position.y;
        float cr = targetDiameter * 1.1f;  // This is the target's diameter not radius, its bigger than you think!

        float dx = x1 - cx;
        float dy = y1 - cy;

        float dist = Mathf.Sqrt(dx * dx + dy * dy);

        float tx, ty;
        if (dist <= cr) // If the point is inside the circle, the math differs. For simplicity, lets just move directly away from the center until we hit the edge.
        {
            Vector2 direction = new Vector2(dx, dy).normalized * cr;

            tx = cx + direction.x;
            ty = cy + direction.y;
        }
        else
        {
            float angle = Mathf.Acos(cr / dist);
            float base_angle = Mathf.Atan2(dy, dx);
            float tangent_angle = base_angle + angle;

            tx = cx + cr * Mathf.Cos(tangent_angle);
            ty = cy + cr * Mathf.Sin(tangent_angle);
        }
        

        Vector3 targetTangent = new Vector3(
            x: tx,
            y: ty,
            z: targetScoreRect.position.z);

        // Round up speed
        float speed = Mathf.Max(Vector2.Distance(markerRect.position, targetTangent), cr) / 0.5f;

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
                to: new Vector3(0, iconRadius + markerDiameter + 0.1f, 0),
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
            to: new Vector3(targetDiameter / markerDiameter * 0.5f, targetDiameter / markerDiameter * 0.5f, targetDiameter / markerDiameter * 0.5f),
            time: 0.5f)
            .PlayAfter(markerMoveUp, IconCountTotal * OrderOffset + 0.0f);
        LTDescr markerOrbit = LeanTweenHelper.Singleton.DynamicOrbitAroundPoint(
                rectTransform: markerRect,
                center: targetScoreRect.position,
                radiusValueTween: LeanTween.value(cr, targetDiameter * 0.6f, 2).PlayAfter(markerMoveToTarget),
                radians: speed * (IconCountTotal * OrderOffset + 0.5f) * targetDiameter * 1.1f,
                time: (IconCountTotal * OrderOffset + 0.5f) * targetDiameter * 1.1f)
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
            .addOnStart(() => Destroy(createdMarker))
            .addOnStart(targetGameEvent.AnimationCompletedCallback)
            .SetCallEventOnStart(new GameEvent() { eventType = GameEvent.EventEnum.PlayPop });
        if (IconOrder == IconCountTotal)
        {
            targetScaleDown.SetCallEventOnComplete(new GameEvent() { eventType = GameEvent.EventEnum.CallShakeOrShowDelta });
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
    public void AssignUserAndRegister(UnityUser relevantUser, int order, int totalNumIcons)
    {
        this.IconOrder = order;
        this.IconCountTotal = totalNumIcons;
        startDelay += order * OrderOffset;
        startEvent.id = relevantUser.Id.ToString();
        CallRegisterForAnimation();
    }
}
