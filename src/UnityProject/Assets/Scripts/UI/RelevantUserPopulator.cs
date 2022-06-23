using Assets.Scripts.Networking.DataModels;
using Assets.Scripts.Views.ViewComponentHandlers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RelevantUserPopulator : MonoBehaviour
{
    public Image TargetLocation;
    public ColorizerHandler Colorizer;
    public Text DisplayName;
    public void Populate(UnityUser user, bool showName, int scoreDelta = 0, int order = 0, int newOrder = 0, int total = 1)
    {
        gameObject.GetComponent<MoveUserScoreProjectileAnimation>().AssignUserAndRegister(user, order, total);
        gameObject.GetComponent<ShowScoreDeltaAnimation>().AssignUserAndRegister(scoreDelta);
        gameObject.GetComponent<ReorderUserIconsAnimation>().AssignUserAndRegister(user, order, newOrder);
        TargetLocation.sprite = user.SelfPortraitSprite;
        Colorizer.UpdateValue(new List<Guid>() { user.Id });
        DisplayName.text = user.DisplayName;
        DisplayName.transform.parent.gameObject.SetActive(showName);

    }
}
