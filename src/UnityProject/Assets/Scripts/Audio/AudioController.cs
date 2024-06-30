using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AudioController : MonoBehaviour
{
    [System.Serializable]
    public class ClipInfo
    {
        public AudioClip clip;
        public GameEvent triggerEvent;
        public float volume;
    }
    public AudioSource audioSource;
    public AudioSource timerAudioSource;

    public List<ClipInfo> audioClips;

    private DateTime lastTimerStop = DateTime.UtcNow;

    public int ClipCooldownInMSec = 10;
    public Dictionary<string, DateTime> ClipDisabledUntil = new Dictionary<string, DateTime>();


    public static AudioController Singleton;

    public void Awake()
    {
        Singleton = this;
    }
    public void Start()
    {
        EventSystem.Singleton.RegisterListener(
            listener: PlayTimer,
            gameEvent: new GameEvent() { eventType = GameEvent.EventEnum.TenSecondsLeft },
            persistant: true,
            oneShot: false);
        EventSystem.Singleton.RegisterListener(
            listener: StopTimer,
            gameEvent: new GameEvent() { eventType = GameEvent.EventEnum.ExitingState },
            persistant: true,
            oneShot: false);

        foreach (ClipInfo clipInfo in audioClips)
        {
            EventSystem.Singleton.RegisterListener(
                listener: (GameEvent gameEvent) =>
                { 
                    // Can play audio clip
                    if (audioSource != null && clipInfo.clip != null)
                    {
                        // Check if recently played audio clip or not
                        if (!ClipDisabledUntil.ContainsKey(clipInfo.clip.name) || ClipDisabledUntil[clipInfo.clip.name] < DateTime.UtcNow)
                        {
                            // Play clip
                            audioSource.PlayOneShot(clipInfo.clip, clipInfo.volume);
                            ClipDisabledUntil[clipInfo.clip.name] = DateTime.UtcNow.AddMilliseconds(ClipCooldownInMSec);
                        }
                    }
                },
                gameEvent: clipInfo.triggerEvent,
                persistant: true,
                oneShot: false
                );
        }
    }

    public void PlayTimer(GameEvent gameEvent = null)
    {
        // Check if we stopped the timer within the last second, deals with race conditions
        if (timerAudioSource != null && lastTimerStop.Add(TimeSpan.FromSeconds(1)) < DateTime.UtcNow)
        {
            timerAudioSource.Play(); 
        }
    }
    public void StopTimer(GameEvent gameEvent = null)
    {
        if (timerAudioSource != null)
        {
            lastTimerStop = DateTime.UtcNow;
            timerAudioSource.Stop();
        }
    }
}
