using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Factory/Graph")]
public class FactoryGraphSO : ScriptableObject
{
    public List<GraphNodeSO> nodes = new List<GraphNodeSO>();
    public List<MachineEdgeSO> edges = new List<MachineEdgeSO>();

#if UNITY_EDITOR
    // Add a Machine Node
    public void AddMachine(MachineNodeSO machineNode, Vector2 position)
    {
        if (machineNode == null) return;

        machineNode.position = position;
        nodes.Add(machineNode);

        AssetDatabase.AddObjectToAsset(machineNode, this);
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
    }

    // Add a Resource Node
    public void AddResource(ResourceNodeSO resourceNode, Vector2 position)
    {
        if (resourceNode == null) return;

        resourceNode.position = position;
        nodes.Add(resourceNode);

        AssetDatabase.AddObjectToAsset(resourceNode, this);
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
    }

    // Add edge
    public void AddConnection(GraphNodeSO fromNode, string fromPort, GraphNodeSO toNode, string toPort)
    {
        if (fromNode == null || toNode == null) return;

        MachineEdgeSO edge = ScriptableObject.CreateInstance<MachineEdgeSO>();
        edge.fromNode = fromNode;
        edge.fromPortName = fromPort;
        edge.toNode = toNode;
        edge.toPortName = toPort;

        edges.Add(edge);

        AssetDatabase.AddObjectToAsset(edge, this);
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
    }
#endif
}
