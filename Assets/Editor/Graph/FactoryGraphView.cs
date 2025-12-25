#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;

public class FactoryGraphView : GraphView
{
    private FactoryGraphWindow window;
    private FactoryGraphSO graphSO;

    private readonly List<MachineEdgeSO> existingEdges = new();

    private static readonly Vector2 DefaultNodeSize = new(220, 140);

    public FactoryGraphView(FactoryGraphWindow wnd)
    {
        window = wnd;

        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        var grid = new GridBackground();
        Insert(0, grid);
        grid.StretchToParentSize();

        graphViewChanged += OnGraphViewChanged;
    }

    #region ReloadGraph
    public void ReloadGraph(FactoryGraphSO graph)
    {
        graphSO = graph;
        DeleteElements(graphElements.ToList());

        if (graphSO == null)
            return;

        // Add nodes
        foreach (var nodeSO in graphSO.nodes)
            AddElement(new GraphNode(nodeSO));

        // Restore edges
        existingEdges.Clear();
        foreach (var edgeSO in graphSO.edges.ToList()) // copy to avoid collection modified
        {
            var fromNodeView = graphElements.OfType<GraphNode>().FirstOrDefault(n => n.NodeSO == edgeSO.fromNode);
            var toNodeView = graphElements.OfType<GraphNode>().FirstOrDefault(n => n.NodeSO == edgeSO.toNode);
            if (fromNodeView == null || toNodeView == null)
                continue;

            var toPort = toNodeView.inputs.FirstOrDefault(p => p.portName == edgeSO.toPortName);
            if (toPort == null)
                continue;

            var edge = fromNodeView.output.ConnectTo(toPort);
            AddElement(edge);
            existingEdges.Add(edgeSO);
        }
    }
    #endregion

    #region OnGraphViewChanged
    private GraphViewChange OnGraphViewChanged(GraphViewChange changes)
    {
        if (graphSO == null) return changes;

        // CREATE EDGES
        if (changes.edgesToCreate != null)
        {
            foreach (var edge in changes.edgesToCreate)
            {
                var fromNode = ((GraphNode)edge.output.node).NodeSO;
                var toNode = ((GraphNode)edge.input.node).NodeSO;

                bool exists = graphSO.edges.Any(e =>
                    e.fromNode == fromNode &&
                    e.toNode == toNode &&
                    e.fromPortName == edge.output.portName &&
                    e.toPortName == edge.input.portName
                );

                if (!exists)
                {
                    graphSO.AddConnection(fromNode, edge.output.portName, toNode, edge.input.portName);
                    EditorUtility.SetDirty(graphSO);
                }
            }
        }

        // DELETE EDGES
        var currentEdges = edges.ToList().Cast<Edge>().ToList();
        foreach (var edgeSO in existingEdges.ToList()) // iterate copy
        {
            bool stillExists = currentEdges.Any(e =>
                edgeSO.fromNode == ((GraphNode)e.output.node).NodeSO &&
                edgeSO.toNode == ((GraphNode)e.input.node).NodeSO &&
                edgeSO.fromPortName == e.output.portName &&
                edgeSO.toPortName == e.input.portName
            );

            if (!stillExists)
            {
                graphSO.edges.Remove(edgeSO);
                AssetDatabase.RemoveObjectFromAsset(edgeSO);
                existingEdges.Remove(edgeSO);
                EditorUtility.SetDirty(graphSO);
                AssetDatabase.SaveAssets();
            }
        }

        FactoryGraphBackup.CreateBackup(graphSO);

        return changes;
    }
    #endregion

    #region GetCompatiblePorts
    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        var compatible = new List<Port>();
        foreach (var port in ports)
        {
            if (port == startPort)
                continue;
            if (port.node == startPort.node)
                continue;

            if ((startPort.direction == Direction.Output && port.direction == Direction.Input) ||
                (startPort.direction == Direction.Input && port.direction == Direction.Output))
            {
                compatible.Add(port);
            }
        }
        return compatible;
    }
    #endregion

    #region Node Helpers
    public Rect GetCenteredRect(Vector2 size)
    {
        var viewRect = contentViewContainer.layout;
        if (viewRect.width <= 0 || viewRect.height <= 0)
            return new Rect(Vector2.zero, size);

        Vector2 center = new(
            viewRect.width * 0.5f - size.x * 0.5f,
            viewRect.height * 0.5f - size.y * 0.5f
        );

        return new Rect(center, size);
    }

    public GraphNode AddNode(GraphNodeSO nodeSO)
    {
        if (nodeSO == null)
            return null;

        if (nodeSO.position == Vector2.zero)
        {
            nodeSO.position = GetCenteredRect(DefaultNodeSize).position;
            EditorUtility.SetDirty(nodeSO);
        }

        graphSO.nodes.Add(nodeSO);
        var nodeView = new GraphNode(nodeSO);
        AddElement(nodeView);
        return nodeView;
    }
    #endregion
}
#endif
