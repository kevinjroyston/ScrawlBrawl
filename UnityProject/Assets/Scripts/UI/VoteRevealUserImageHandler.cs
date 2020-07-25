using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VoteRevealUserImageHandler : MonoBehaviour
{
    public GameObject PlayerIconPrefab;
    public GameObject DropZone;

    public void Start()
    {
    }

    public void HandleUsers(IReadOnlyList<User> users)
    {
        ClearUsers();
        if (users == null || users.Count <= 0)
        {
            return;
        }

        for (int i = 0; i < users.Count; i++)
        {
            InstantiateRelevantUser(DropZone.transform, users[i], i + 1, users.Count);      
        }
    }

    private void InstantiateRelevantUser(Transform parent, User user, int order = 1, int total = 1)
    {
        var relUser = Instantiate(PlayerIconPrefab, parent);
        relUser.transform.localScale = new Vector3(1f, 1f, 1f);
        relUser.transform.localPosition = Vector3.zero;

        relUser.GetComponent<RelevantUserPopulator>().Populate(user, true, order, total);
    }

    private void ClearUsers()
    {
        foreach (Transform child in DropZone.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }
}
