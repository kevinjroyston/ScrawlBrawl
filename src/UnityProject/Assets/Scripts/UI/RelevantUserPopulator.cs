using Assets.Scripts.Networking.DataModels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RelevantUserPopulator : MonoBehaviour
{
    public Image TargetLocation;
    public Colorizer Colorizer;
    public Text DisplayName;
    public void Populate(UnityUser user, bool showName, int scoreDelta = 0, int order = 1, int total = 1)
    {
        gameObject.GetComponent<MoveUserIconAnimation>().AssignUserAndRegister(user, order, total);
        gameObject.GetComponent<ShowScoreDeltaAnimation>().AssignUserAndRegister(scoreDelta);
        TargetLocation.sprite = user.SelfPortraitSprite;
        Colorizer.RefreshColor(user.DisplayName);
        DisplayName.text = user.DisplayName;
        DisplayName.transform.parent.gameObject.SetActive(showName);
    }
}
