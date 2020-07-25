using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioSource timerAudioSource;
    public AudioClip startDingClip;
    public AudioClip userSubmitClip;
    public AudioClip wooshClip;
    public AudioClip popClip;
    public AudioClip drumRollClip;
    //public AudioClip endDingClip;
    

    public static AudioController Singleton;

    public void Awake()
    {
        Singleton = this;
    }
    public void Start()
    {
        EventSystem.Singleton.RegisterListener(
            listener: PlayStartDing,
            gameEvent: new GameEvent() { eventType = GameEvent.EventEnum.EnteredState },
            persistant: true);
        EventSystem.Singleton.RegisterListener(
            listener: PlayTimer,
            gameEvent: new GameEvent() { eventType = GameEvent.EventEnum.TenSecondsLeft },
            persistant: true);
        EventSystem.Singleton.RegisterListener(
            listener: StopTimer,
            gameEvent: new GameEvent() { eventType = GameEvent.EventEnum.ExitingState },
            persistant: true);
        EventSystem.Singleton.RegisterListener(
            listener: PlayUserSubmit,
            gameEvent: new GameEvent() { eventType = GameEvent.EventEnum.UserSubmitted },
            persistant: true);
        EventSystem.Singleton.RegisterListener(
            listener: PlayWoosh,
            gameEvent: new GameEvent() { eventType = GameEvent.EventEnum.VoteRevealBubbleMove },
            persistant: true);
        EventSystem.Singleton.RegisterListener(
            listener: PlayPop,
            gameEvent: new GameEvent() { eventType = GameEvent.EventEnum.PlayPop },
            persistant: true);
        EventSystem.Singleton.RegisterListener(
            listener: PlayDrumRoll,
            gameEvent: new GameEvent() { eventType = GameEvent.EventEnum.PlayDrumRoll },
            persistant: true);
    }

    public void PlayStartDing(GameEvent gameEvent = null)
    {
        if (audioSource!=null && startDingClip != null)
        {
            audioSource.PlayOneShot(startDingClip, 0.2f);
        }     
    }
    /*public void PlayEndDing()
    {
        if (audioSource != null && endDingClip != null)
        {
            audioSource.PlayOneShot(endDingClip);
        }
    }*/
    public void PlayTimer(GameEvent gameEvent = null)
    {
        if (timerAudioSource != null)
        {
            timerAudioSource.Play(); 
        }
    }
    public void StopTimer(GameEvent gameEvent = null)
    {
        if (timerAudioSource != null)
        {
            timerAudioSource.Stop();
        }
    }
    public void PlayUserSubmit(GameEvent gameEvent = null)
    {
        if (audioSource != null && userSubmitClip != null)
        {
            audioSource.PlayOneShot(userSubmitClip);
        }
    }
    public void PlayWoosh(GameEvent gameEvent = null)
    {
        if (audioSource != null && wooshClip != null)
        {
            audioSource.PlayOneShot(wooshClip, 0.2f);
        }
    }
    public void PlayPop(GameEvent gameEvent = null)
    {
        if (audioSource != null && popClip != null)
        {
            audioSource.PlayOneShot(popClip);
        }
    }
    public void PlayDrumRoll(GameEvent gameEvent = null)
    {
        if (audioSource != null && drumRollClip != null)
        {
            audioSource.PlayOneShot(drumRollClip, 0.5f);
        }
    }

}
