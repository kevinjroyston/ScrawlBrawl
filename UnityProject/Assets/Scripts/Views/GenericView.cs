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

        if (Title?.GetComponent<Text>() != null)
        {
            var title = Title.GetComponent<Text>();
            if(!string.IsNullOrWhiteSpace(UnityView._Title))
            {
                title.enabled = true;
                title.text = UnityView._Title;
            }
            else
            {
                title.enabled = false;
            }
        }

        if (Instructions?.GetComponent<Text>() != null)
        {
            var instructions = Instructions.GetComponent<Text>();
            if (!string.IsNullOrWhiteSpace(UnityView._Instructions))
            {
                instructions.enabled = true;
                instructions.text = UnityView._Instructions;
            }
            else
            {
                instructions.enabled = false;
            }
        }

        if (ImagePrefab != null && ImageDropZone != null && (UnityView?._UnityImages?.Any() ?? false))
        {
            // TODO: below causes sprite to be created an extra time. consider caching all the base64 sprites or using RawImage.
            LoadAllImages(UnityView._UnityImages.ToList());
        }
    }

    private void LoadAllImages(List<UnityImage> images)
    {
        // Instantiate more image objects if needed
        var breakoutCounter = 0;
        while (images.Count > ImageDropZone.transform.childCount && breakoutCounter++ < 50)
        {
            Instantiate(ImagePrefab, ImageDropZone.transform);
        }

        // TODO: Delete excess image objects if needed
        /* 
        breakoutCounter = 0;
        while (images.Count < ImageDropZone.transform.childCount && breakoutCounter++ < 50)
        {
            Destroy(ImageDropZone.transform.GetChild(ImageDropZone.transform.childCount - 1));
        }*/

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
