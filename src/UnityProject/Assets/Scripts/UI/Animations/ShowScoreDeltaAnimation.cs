using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowScoreDeltaAnimation : AnimationBase
{
    public Text scoreDeltaTextPrefab;
    private string scoreDelta = "+0";
    public override List<LTDescr> Animate(GameEvent gameEvent)
    {
        Text createdText = Instantiate(scoreDeltaTextPrefab, rect);
        createdText.text = scoreDelta;
        RectTransform textTransform = createdText.rectTransform;
        textTransform.localScale = Vector3.zero;
        Vector2 iconRadiusVector = rect.localToWorldMatrix.MultiplyVector(new Vector2(rect.rect.width, rect.rect.height));
        float iconRadius = Mathf.Min(iconRadiusVector.x, iconRadiusVector.y) / 2;

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
        LTDescr scaleTextUp = LeanTween.scale(
                rectTrans: textTransform,
                to: Vector3.one,
                time: 0.2f)
            .setEaseOutBack()
            .PlayAfter(iconScaleDown);
        LTDescr moveTextUp = LeanTweenHelper.Singleton.UIMoveRelative(
                rectTransform: textTransform,
                to: new Vector3(0, iconRadius + 0.2f, 0),
                time: 0.2f)
            .setEaseOutBack()
            .PlayAfter(iconScaleDown)
            .SetCallEventOnComplete(new GameEvent() { eventType = GameEvent.EventEnum.ReorderIcons });
        LTDescr shakeText = LeanTweenHelper.Singleton.UIShake(
                rectTransform: textTransform,
                magnitude: 0.05f,
                rateOfChange: 0.001f,
                frequency: 0.7f,
                time: 3f)
            .PlayAfter(moveTextUp);
        LTDescr scaleTextDown = LeanTween.scale(
                rectTrans: textTransform,
                to: Vector3.zero,
                time: 0.2f)
            .setEaseInBack()
            .PlayAfter(shakeText);
        LTDescr moveTextDown = LeanTweenHelper.Singleton.UIMoveRelative(
                rectTransform: textTransform,
                to: new Vector3(0, -2, 0),
                time: 0.2f)
            .setEaseInBack()
            .PlayAfter(shakeText);

        return new List<LTDescr>()
        {
            iconScaleDown,
            iconScaleUp,
            scaleTextUp,
            moveTextUp,
            shakeText,
            scaleTextDown,
            moveTextDown
        };
    }

    public void AssignUserAndRegister(int scoreDeltaInt)
    {
        if (scoreDeltaInt >= 0)
        {
            scoreDelta = "+" + scoreDeltaInt;
        }
        //startEvent.id = relevantUser.UserId.ToString();
        startEvent.eventType = GameEvent.EventEnum.ShowDeltaScores;
        CallRegisterForAnimation();    
    }
}
