#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;

public class FactoryGraphView : GraphView
{
    private FactoryGraphWindow window;
    private FactoryGraphSO graphSO;

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
    }

    public void LoadGraph(FactoryGraphSO graph)
    {
        graphSO = graph;

        DeleteElements(graphElements.ToList());

        // Add nodes
        foreach (var node in graph.nodes)
        {
            var viewNode = new GraphNode(node);
            AddElement(viewNode);
        }

        // Add edges
        foreach (var edge in graph.edges)
        {
            var fromNodeView = graphElements.OfType<GraphNode>()
                .First(n => n.NodeSO == edge.fromNode);
            var toNodeView = graphElements.OfType<GraphNode>()
                .First(n => n.NodeSO == edge.toNode);

            var graphEdge = fromNodeView.output.ConnectTo(toNodeView.input);
            AddElement(graphEdge);
        }
    }
}
#endif
