using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Factory/Machine")]
public class MachineSO : ScriptableObject
{
    public string machineName;
    public float productionRate;

    // Multiple inputs and outputs
    public List<ResourceAmount> inputs = new List<ResourceAmount>();
    public List<ResourceAmount> outputs = new List<ResourceAmount>();
}

[System.Serializable]
public class ResourceAmount
{
    public string resourceName;
    public int amount;
}
