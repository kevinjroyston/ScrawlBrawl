using Assets.Scripts.Networking.DataModels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StackTextPopulator : MonoBehaviour
{
    public Text MainText;
    public GameObject OwnerCircle;
    public Image OwnerImage;
    public Image ColorizeImage;

    public void SetMainText(string text)
    {
        MainText.text = text;
    }
    public void SetOwnerText(UnityUser user)
    {
        if (user == null)
        {
            OwnerCircle.SetActive(false);
        }
        else
        {
            OwnerCircle.SetActive(true);
            OwnerImage.sprite = user.SelfPortraitSprite;
            ColorizeImage.color = ColorizerManager.Singleton.GetColor(user.Id.ToString());
        }
    }
}
