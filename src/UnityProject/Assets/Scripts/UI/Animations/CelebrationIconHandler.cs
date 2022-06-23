using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Networking.DataModels;
using Assets.Scripts.Views.ViewComponentHandlers;

public class CelebrationIconHandler : MonoBehaviour
{
    public List<ParticleSystem> Top3Celebrations = new List<ParticleSystem>();
    public void SetPosition(int position)
    {
        // Only care if we are 1,2,3
        if (position > 3 || position <= 0)
        {
            DeactivateCelebrations();
            return;
        }
        Top3Celebrations[position-1].Play();
    }

    private void DeactivateCelebrations()
    {
        foreach (var obj in Top3Celebrations)
        {
            obj.Stop();
        }
    }
}
