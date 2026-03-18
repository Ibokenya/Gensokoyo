using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateItem : MonoBehaviour
{
    [Header("物品预制体列表")]
    public List<GameObject> ItemPrefabs;
    [Header("初始化物品数量列表（对应物品预制体列表）")]
    public List<int> ItemCount;

    void OnEnable()
    {
        InitItemPool();
    }
    
    /// <summary>
    /// 初始化物品池
    /// </summary>
    private void InitItemPool()
    {
        for (int i = 0; i < ItemPrefabs.Count; i++)
        {
            Global_ObjectPool.Instance.InitPool(ItemPrefabs[i], ItemCount[i]);
        }
    }
}
