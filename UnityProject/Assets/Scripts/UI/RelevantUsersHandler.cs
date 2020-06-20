using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelevantUsersHandler : MonoBehaviour
{
    public GameObject RelevantUserPrefab;
    public GameObject RelevantUserLeftDropZone;
    public GameObject RelevantUserRightDropZone;
    public GameObject RelevantUserSingleDropZone;

    public void Start()
    {
        var grids = GetComponentsInChildren<AutoScaleGridLayoutGroup>();
        foreach (AutoScaleGridLayoutGroup grid in grids)
        {
            grid.aspectRatio = 2.0f;
        }
    }

    public void HandleRelevantUsers(IReadOnlyList<User> relevantUsers, bool voteCountEnabled)
    {
        return;
    }
    /*
            // TODO: fix this implementation
            ClearUsers();
            if (relevantUsers == null || relevantUsers.Count <= 0)
            {
                return;
            }

            if (voteCountEnabled)
            {
                for (int i = 0; i < relevantUsers.Count; i++)
                {
                    InstantiateRelevantUser(
                        i % 2 == 0 ? RelevantUserLeftDropZone.transform : RelevantUserRightDropZone.transform,
                        relevantUsers[i]);
                }
            }
            else
            {
                for (int i = 0; i < relevantUsers.Count; i++)
                {
                    InstantiateRelevantUser(RelevantUserSingleDropZone.transform, relevantUsers[i]);
                }
            }
        }
        private void InstantiateRelevantUser(Transform parent, User user)
        {
            var relUser = Instantiate(RelevantUserPrefab, parent);
            relUser.transform.localScale = new Vector3(1f, 1f, 1f);
            relUser.transform.localPosition = Vector3.zero;

            relUser.GetComponent<RelevantUserPopulator>().Populate(user);
        }*/

        private void ClearUsers()
    {
        foreach (Transform child in RelevantUserLeftDropZone.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        foreach (Transform child in RelevantUserRightDropZone.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        foreach (Transform child in RelevantUserSingleDropZone.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }
}
