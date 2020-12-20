using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Assets.Scripts.Networking.DataModels;
using System.Linq;

public class UnityObjectTypeToPrefabStaticMap : MonoBehaviour
{
    public static UnityObjectTypeToPrefabStaticMap Singleton;
    public List<UnityObjectTypeToPrefab> TypeToPrefabList;
    public Dictionary<UnityObjectType, GameObject> Mapping { get; set; }
    public void Awake()
    {
        Singleton = this;
        Mapping = TypeToPrefabList.GroupBy(item => item.Type).ToDictionary(item => item.Key, item => item.First().Prefab);
    }

    [Serializable]
    public class UnityObjectTypeToPrefab
    {
        public UnityObjectType Type;
        public GameObject Prefab;
    }

}
