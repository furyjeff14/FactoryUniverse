#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class GraphNode : Node
{
    public GraphNodeSO NodeSO { get; private set; }
    public Port input;
    public Port output;

    public GraphNode(GraphNodeSO node)
    {
        NodeSO = node;
        title = node.name;

        // Example ports
        input = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(float));
        input.portName = "Input";
        output = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(float));
        output.portName = "Output";

        inputContainer.Add(input);
        outputContainer.Add(output);

        RefreshExpandedState();
        RefreshPorts();

        SetPosition(new Rect(node.position, Vector2.zero));
    }
}
#endif
