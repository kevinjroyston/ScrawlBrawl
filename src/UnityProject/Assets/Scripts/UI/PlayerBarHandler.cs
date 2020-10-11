using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerBarHandler : MonoBehaviour
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

        int waitingCount = users.Where(user => user.Status == User.UserStatus.AnsweringPrompts).Count();

        for (int i = 0; i < users.Count; i++)
        {
            if (users[i].Status == User.UserStatus.AnsweringPrompts)
            {
                InstantiateRelevantUser(DropZone.transform, users[i], waitingCount <= 3);
            }
        }
    }

    private void InstantiateRelevantUser(Transform parent, User user, bool showName)
    {
        var relUser = Instantiate(PlayerIconPrefab, parent);
        relUser.transform.localScale = new Vector3(1f, 1f, 1f);
        relUser.transform.localPosition = Vector3.zero;

        relUser.GetComponent<RelevantUserPopulator>().Populate(user, showName);
    }

    private void ClearUsers()
    {
        foreach (Transform child in DropZone.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }
}
