using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioSource timerAudioSource;
    public AudioClip startDingClip;
    public AudioClip userSubmitClip;
    //public AudioClip endDingClip;
    

    public static AudioController Singleton;

    public void Awake()
    {
        Singleton = this;
    }

    public void PlayStartDing()
    {
        if (audioSource!=null && startDingClip != null)
        {
            audioSource.PlayOneShot(startDingClip);
        }     
    }
    /*public void PlayEndDing()
    {
        if (audioSource != null && endDingClip != null)
        {
            audioSource.PlayOneShot(endDingClip);
        }
    }*/
    public void PlayTimer()
    {
        if (timerAudioSource != null)
        {
            timerAudioSource.Play(); 
        }
    }
    public void StopTimer()
    {
        if (timerAudioSource != null)
        {
            timerAudioSource.Stop();
        }
    }
    public void PlayUserSubmit()
    {
        if (audioSource != null && userSubmitClip != null)
        {
            audioSource.PlayOneShot(userSubmitClip);
        }
    }
}
