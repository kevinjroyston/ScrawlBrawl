using Assets.Scripts.Networking.DataModels;
using Assets.Scripts.Views.DataModels;
using Assets.Scripts.Views.Interfaces;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static TypeEnums;

public class PlayerBarHandler : MonoBehaviour, HandlerInterface
{
    public GameObject DropZone;
    public List<HandlerId> HandlerIds => HandlerType.UsersList.ToHandlerIdList();
    public HandlerScope Scope => HandlerScope.View;

    public void UpdateValue(UsersListHolder usersHolder)
    {
        List<UnityUser> users = usersHolder.Users;
        bool isRevealing = usersHolder.IsRevealing;
        var childCount = gameObject.transform.childCount;
        ClearUsers();
        if (users == null || users.Count <= 0)
        {
            return;
        }

        // If we are revealing, order users by their score
        Dictionary <UnityUser, int> newOrders = new Dictionary<UnityUser, int>();
        if (isRevealing){
            users = users.OrderByDescending(user => user.Score - user.ScoreDeltaReveal).ToList();
            var newOrdersList = users.OrderByDescending(user => user.Score).ToList();
            int index = 0;
            foreach(UnityUser user in newOrdersList){
                index++;
                newOrders[user] = index;
            }
        }

        int waitingCount = users.Where(user => user.Status == UserStatus.AnsweringPrompts).Count();

        for (int i = 0; i < users.Count; i++)
        {
            if (isRevealing)
            {
                InstantiateRelevantUser(
                    user: users[i],
                    showName: true,
                    scoreDelta: users[i].ScoreDeltaReveal,
                    order: i + 1,
                    newOrder: newOrders[users[i]],
                    total: users.Count);
            }
            else
            {
                if (users[i].Status == UserStatus.AnsweringPrompts)
                {
                    InstantiateRelevantUser(
                        user: users[i],
                        showName: waitingCount <= 3);
                }
            }
        }
    }


    private void InstantiateRelevantUser(UnityUser user, bool showName = true, int scoreDelta = 0, int order = -1, int newOrder = -1, int total = 1)
    {
        GameObject playerIconPrefab = PrefabLookup.Singleton.Mapping[PrefabLookup.PrefabType.UserIcon];
        var relUser = Instantiate(playerIconPrefab, DropZone.transform);
        relUser.transform.localScale = new Vector3(1f, 1f, 1f);
        relUser.transform.localPosition = Vector3.zero;

        relUser.GetComponent<RelevantUserPopulator>().Populate(user, showName, scoreDelta, order, newOrder, total);
    }

    private void ClearUsers()
    {
        foreach (Transform child in DropZone.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }

    public void UpdateValue(List<object> objects)
    {
        this.UpdateValue((UsersListHolder) objects[0]);
    }
}
