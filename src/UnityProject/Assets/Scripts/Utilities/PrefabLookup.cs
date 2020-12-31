using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PrefabLookup : MonoBehaviour
{
    public static PrefabLookup Singleton;
    public List<PrefabTypeObjectCombo> TypeToPrefabList;
    public Dictionary<PrefabType, GameObject> Mapping { get; set; }

    public void Awake()
    {
        Singleton = this;
        Mapping = TypeToPrefabList.GroupBy(item => item.Type).ToDictionary(item => item.Key, item => item.First().Prefab);
    }

    [Serializable]
    public class PrefabTypeObjectCombo
    {
        public PrefabType Type;
        public GameObject Prefab;
    }

    public enum PrefabType
    {
        UserIcon,
        GameIcon,
        ScoreDeltaText,
        ScoreProjectile,
        SpriteGrid,
        SpriteObject
    }
}
