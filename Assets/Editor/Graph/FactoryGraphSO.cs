using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using System;



#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Factory/Graph")]
public class FactoryGraphSO : ScriptableObject
{
    public List<GraphNodeSO> nodes = new List<GraphNodeSO>();
    public List<MachineEdgeSO> edges = new List<MachineEdgeSO>();

#if UNITY_EDITOR

    public event Action OnGraphChanged;
    private void OnValidate()
    {
        OnGraphChanged?.Invoke();
    }

    // Add a Machine Node
    public FactoryGraphView graphView; // set this from your editor window

    public void AddMachine(MachineNodeSO machineNode, Vector2 position)
    {
        if (machineNode == null) return;

        machineNode.position = position;
        nodes.Add(machineNode);

        AssetDatabase.AddObjectToAsset(machineNode, this);
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Notify GraphView
        graphView?.ReloadGraph(this);
    }

    public void AddConnection(GraphNodeSO fromNode, string fromPort, GraphNodeSO toNode, string toPort)
    {
        if (fromNode == null || toNode == null)
            return;

        var edgeSO = ScriptableObject.CreateInstance<MachineEdgeSO>();
        edgeSO.fromNode = fromNode;
        edgeSO.fromPortName = fromPort;
        edgeSO.toNode = toNode;
        edgeSO.toPortName = toPort;

        edges.Add(edgeSO);

        // Important: add it to the FactoryGraphSO asset so it persists
        AssetDatabase.AddObjectToAsset(edgeSO, this);
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
    }


#endif
}
