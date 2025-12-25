using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Factory/Build Item")]
public class BuildItemSO : ScriptableObject
{
    public string itemName;
    public GameObject prefab;

    // Optional machine data
    public MachineNodeSO machineData;

    [Header("Conveyor Belt Settings (Optional)")]
    public bool isBelt = false;
    public float beltSpeed = 2f;
    public float beltCapacity = 50f;

    [System.Serializable]
    public struct Cost
    {
        public string resourceName;
        public float amount;
    }

    public List<Cost> cost = new();
}
