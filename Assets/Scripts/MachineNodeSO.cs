using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Factory/Machine Node")]
public class MachineNodeSO : GraphNodeSO
{
    public string machineName;

    // Inputs and outputs reference ResourceNodeSO
    public List<ResourceNodeSO> inputs = new List<ResourceNodeSO>();
    public List<ResourceNodeSO> outputs = new List<ResourceNodeSO>();
}
