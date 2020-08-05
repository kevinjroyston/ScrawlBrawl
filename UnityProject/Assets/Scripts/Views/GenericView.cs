using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GenericView : ITVView
{
    private UnityView UnityView { get; set; }
    public List<TVScreenId> ScreenId;
    public GameObject Title = null;
    public GameObject Instructions = null;
    public GameObject ImageDropZone = null;
    public GameObject ImagePrefab = null;
    public GameObject PlayerBar = null;
    public GameObject VoteReavealBar = null;
    public GameObject TimerUI = null;
    public Camera BlurCamera = null;

    void Awake()
    {
        foreach(TVScreenId id in ScreenId)
        {
            ViewManager.Singleton.RegisterTVView(id, this);
        }
        gameObject.SetActive(false);
    }

    void Start()
    {
    }

    public override void EnterView(UnityView view)
    {
        base.EnterView(view);
        UnityView = view;
        gameObject.SetActive(true);
        if (Title?.GetComponentInChildren<Text>() != null)
        {
            var title = Title.GetComponentInChildren<Text>();
            if(!string.IsNullOrWhiteSpace(UnityView?._Title))
            {
                title.enabled = true;
                title.text = UnityView._Title;
                Title.SetActive(true);
            }
            else
            {
                title.enabled = false;
                Title.SetActive(false);
            }
        }

        if (Instructions?.GetComponent<Text>() != null)
        {
            var instructions = Instructions.GetComponent<Text>();
            if (!string.IsNullOrWhiteSpace(UnityView?._Instructions))
            {
                instructions.enabled = true;
                instructions.text = UnityView._Instructions;
                Instructions.SetActive(true);
            }
            else
            {
                instructions.enabled = false;
                Instructions.SetActive(false);
            }
        }

        if (ImagePrefab != null && ImageDropZone != null && (UnityView?._UnityImages?.Any() ?? false))
        {
            // TODO: below causes sprite to be created an extra time. consider caching all the base64 sprites or using RawImage.
            LoadAllImages(UnityView._UnityImages.ToList());
            ImageDropZone.SetActive(true);
        }
        else
        {
            ImageDropZone.SetActive(false);
        }

        bool voteRevealBar = false;
        if (VoteReavealBar?.GetComponent<VoteRevealUserImageHandler>() != null)
        {
            var voteRevealHandler = VoteReavealBar.GetComponent<VoteRevealUserImageHandler>();
            if (UnityView?._VoteRevealUsers != null 
                && UnityView._VoteRevealUsers.Count > 0
                && UnityView._UserIdToDeltaScores != null)
            {
                voteRevealBar = true;
                voteRevealHandler.HandleUsers(UnityView._VoteRevealUsers, UnityView._UserIdToDeltaScores);
                VoteReavealBar.SetActive(true);
            }
            else
            {
                VoteReavealBar.SetActive(false);
            }
        }

        if (PlayerBar?.GetComponent<PlayerBarHandler>() != null)
        {
            var playerBar = PlayerBar.GetComponent<PlayerBarHandler>();
            if (UnityView?._Users != null && UnityView._Users.Count > 0 && !voteRevealBar)
            {
                playerBar.HandleUsers(UnityView._Users);
                PlayerBar.SetActive(true);
            }
            else
            {
                PlayerBar.SetActive(false);
            }
        }

        if (TimerUI?.GetComponent<TimerUI>() != null)
        {
            var timer = TimerUI.GetComponent<TimerUI>();
            if (UnityView._StateEndTime != null && UnityView._StateEndTime > UnityView.ServerTime)
            {
                timer.UpdateTime(UnityView._StateEndTime.Value, UnityView.ServerTime);
                TimerUI.SetActive(true);
            }
            else
            {
                TimerUI.SetActive(false);
            }
        }

        if(BlurCamera?.GetComponent<BlurController>() != null)
        {
            if (UnityView?._Options != null && UnityView?._Options._BlurAnimate != null)
            {
                BlurCamera.GetComponent<BlurController>().UpdateBlur(
                    startValue: UnityView._Options._BlurAnimate._StartValue,
                    endValue: UnityView._Options._BlurAnimate._EndValue,
                    startTime: UnityView._Options._BlurAnimate._StartTime,
                    endTime: UnityView._Options._BlurAnimate._EndTime,
                    serverCurrentTime: UnityView.ServerTime);
            }
            else
            {
                BlurCamera.GetComponent<BlurController>().RemoveBlur();
            }
        }
    }

    private void LoadAllImages(List<UnityImage> images)
    {
        for (int i = images.Count; i < ImageDropZone.transform.childCount; i++)
        {
            Destroy(ImageDropZone.transform.GetChild(i).gameObject);
        }

        if (images.Count == 0)
        {
            return;
        }
        // Instantiate more image objects if needed
        var breakoutCounter = 0;
        while (images.Count > ImageDropZone.transform.childCount && breakoutCounter++ < 100)
        {
            Instantiate(ImagePrefab, ImageDropZone.transform);
        }

        bool isRevealingImage = false;
        // Set the image object sprites accordingly.
        for (int i = 0; i < images.Count; i++)
        {
            if (images[i]._VoteRevealOptions?._RevealThisImage ?? false)
            {
                isRevealingImage = true;
            }
            images[i].Options = UnityView._Options;
            ImageDropZone.transform.GetChild(i).GetComponent<UnityObjectHandlerInterface>().UnityImage = images[i];
        }
        if (isRevealingImage) // only shake the images if one of them is going to be revealed
        {
            EventSystem.Singleton.RegisterListener(
                listener: (GameEvent gameEvent) => EventSystem.Singleton.PublishEvent(new GameEvent() { eventType = GameEvent.EventEnum.ShakeRevealImages }, allowDuplicates: false),
                gameEvent: new GameEvent() { eventType = GameEvent.EventEnum.CallShakeRevealImages });
        }
        else
        {
            EventSystem.Singleton.RegisterListener(
               listener: (GameEvent gameEvent) => EventSystem.Singleton.PublishEvent(new GameEvent() { eventType = GameEvent.EventEnum.ShowDeltaScores }, allowDuplicates: false),
               gameEvent: new GameEvent() { eventType = GameEvent.EventEnum.CallShakeRevealImages });
        }
    }

    public override void ExitView()
    {
        base.ExitView();
        gameObject.SetActive(false);
    }
}
