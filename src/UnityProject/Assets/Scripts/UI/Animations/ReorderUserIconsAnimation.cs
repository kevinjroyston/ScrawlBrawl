using Assets.Scripts.Networking.DataModels;
using Assets.Scripts.Views.ViewComponentHandlers;
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.UI;

public class ReorderUserIconsAnimation : AnimationBase
{
    private int StartIconOrder;
    private int EndIconOrder;
    private float OrderOffset = 0.1f;
    private CelebrationIconHandler CelebrationIconHandler;
    public override List<LTDescr> Animate(GameEvent gameEvent)
    {
        // Turn off all celebrations
        CelebrationIconHandler.SetPosition(-1);

        //float speed = Vector2.Distance(transform.position, this.EndPosition) / 0.5f;
        //GetComponent<LayoutElement>().ignoreLayout = true;
        this.EndPosition = transform.parent.GetChild(EndIconOrder-1).GetComponent<RectTransform>().position;
        var rectTransform = this.GetComponent<RectTransform>();
        //var localEndPos = rectTransform.InverseTransformPoint(this.EndPosition);
        Debug.Log($"Start order: {StartIconOrder}, End: {EndIconOrder}, StartPosition: {rectTransform.position}, EndPosition:{this.EndPosition}");
        LTDescr moveToNewSpot = LeanTweenHelper.Singleton.UIMove(
                rectTransform: rectTransform,
                to: this.EndPosition,
                time: 0.5f)
            .setEaseInOutBack()
            .addOnComplete(() => CelebrationIconHandler.SetPosition(this.EndIconOrder));

        // Re-enable celebrations per new orders

        List<LTDescr> animations = new List<LTDescr>()
        {
            moveToNewSpot
        };
        return animations;
    }

    private Vector3 EndPosition;
    public void AssignUserAndRegister(UnityUser relevantUser, int oldOrder, int newOrder)
    {
        CelebrationIconHandler = GetComponent<CelebrationIconHandler>();

        // Turn on celebrations for user based on old order
        CelebrationIconHandler.SetPosition(oldOrder);

        this.StartIconOrder = oldOrder;
        this.EndIconOrder = newOrder;
        //startDelay += oldOrder * OrderOffset;
        startEvent.id = relevantUser.Id.ToString();
        // not sure if we need to bring back endevent here, it was getting multi key errors
        
        startEvent.eventType = GameEvent.EventEnum.ReorderIcons;
        CallRegisterForAnimation();
    }
}