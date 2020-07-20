using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RelevantUserPopulator : MonoBehaviour
{
    public Image TargetLocation;
    public Colorizer Colorizer;
    public Text DisplayName;
    public void Populate(User user, bool showName)
    {
        gameObject.GetComponent<MoveUserIconAnimation>().AssignUserAndRegister(user);
        TargetLocation.sprite = user.SelfPortraitSprite;
        Colorizer.RefreshColor(user.DisplayName);
        DisplayName.text = user.DisplayName;
        DisplayName.transform.parent.gameObject.SetActive(showName);
    }
}
