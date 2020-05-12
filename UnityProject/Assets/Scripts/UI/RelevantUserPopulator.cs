using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RelevantUserPopulator : MonoBehaviour
{
    public Image TargetLocation;
    public Colorizer Colorizer;

    public void Populate(User user)
    {
        TargetLocation.sprite = user.SelfPortraitSprite;
        Colorizer.RefreshColor(user.DisplayName);
    }
}
