using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
                    if (audioSource != null && clipInfo.clip != null)
                    {
                        audioSource.PlayOneShot(clipInfo.clip, clipInfo.volume);
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



}
