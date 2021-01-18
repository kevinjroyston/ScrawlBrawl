using Assets.Scripts.Networking.DataModels;
using System;
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

    public void HandleUsers(List<UnityUser> users, IDictionary<string, int> userIdsToScoreDelta)
    {
        ClearUsers();
        if (users == null || users.Count <= 0)
        {
            return;
        }

        for (int i = 0; i < users.Count; i++)
        {
            if (userIdsToScoreDelta.ContainsKey(users[i].Id.ToString()))
            {
                InstantiateRelevantUser(DropZone.transform, users[i], userIdsToScoreDelta[users[i].Id.ToString()], i + 1, users.Count);
            }
            else
            {
                InstantiateRelevantUser(DropZone.transform, users[i], 0, i + 1, users.Count);
            }
        }
    }

    private void InstantiateRelevantUser(Transform parent, UnityUser user, int scoreDelta, int order = 1, int total = 1)
    {
        var relUser = Instantiate(PlayerIconPrefab, parent);
        relUser.transform.localScale = new Vector3(1f, 1f, 1f);
        relUser.transform.localPosition = Vector3.zero;

        //relUser.GetComponent<RelevantUserPopulator>().Populate(user, true, scoreDelta, order, total);
    }

    private void ClearUsers()
    {
        foreach (Transform child in DropZone.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }
}
