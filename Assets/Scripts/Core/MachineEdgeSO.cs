using UnityEngine;

[CreateAssetMenu(menuName = "Factory/Machine Edge")]
public class MachineEdgeSO : ScriptableObject
{
    public GraphNodeSO fromNode;    // Can be MachineNodeSO or ResourceNodeSO
    public string fromPortName;

    public GraphNodeSO toNode;    // Only machines can be target
    public string toPortName;
}
