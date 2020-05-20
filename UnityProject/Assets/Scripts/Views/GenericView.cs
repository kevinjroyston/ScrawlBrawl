using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GenericView : MonoBehaviour, ITVView
{
    private UnityView UnityView { get; set; }
    public List<TVScreenId> ScreenId;
    public GameObject Title = null;
    public GameObject Instructions = null;
    public GameObject ImageDropZone = null;
    public GameObject ImagePrefab = null;
    public GameObject PlayerBar = null;
    public GameObject TimerUI = null;

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

    public void EnterView(UnityView view)
    {
        UnityView = view;
        gameObject.SetActive(true);

        if (Title?.GetComponentInChildren<Text>() != null)
        {
            var title = Title.GetComponentInChildren<Text>();
            if(!string.IsNullOrWhiteSpace(UnityView._Title))
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
            if (!string.IsNullOrWhiteSpace(UnityView._Instructions))
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

        if (PlayerBar?.GetComponent<PlayerBarHandler>() != null)
        {
            var playerBar = PlayerBar.GetComponent<PlayerBarHandler>();
            if (UnityView._Users != null && UnityView._Users.Count > 0)
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
    }

    private void LoadAllImages(List<UnityImage> images)
    {
        if(images.Count == 0)
        {
            return;
        }
        // Instantiate more image objects if needed
        var breakoutCounter = 0;
        while (images.Count > ImageDropZone.transform.childCount && breakoutCounter++ < 50)
        {
            Instantiate(ImagePrefab, ImageDropZone.transform);
        }

        for ( int i = images.Count; i < ImageDropZone.transform.childCount; i++)
        {
            Destroy(ImageDropZone.transform.GetChild(i));
        }

        // Set the image object sprites accordingly.
        for (int i = 0; i < ImageDropZone.transform.childCount; i++)
        {
            ImageDropZone.transform.GetChild(i).GetComponent<ImageHandler>().UnityImage = images[i];
        }
    }

    public void ExitView()
    {
        gameObject.SetActive(false);
    }
}
