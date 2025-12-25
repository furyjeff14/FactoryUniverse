using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Factory/Build Item")]
public class BuildItemSO : ScriptableObject
{
    public string itemName;
    public GameObject prefab;
    public MachineNodeSO machineData;

    [System.Serializable]
    public struct Cost
    {
        public string resourceName;
        public float amount;
    }

    public List<Cost> cost = new();
}
