#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;

public class GraphNode : Node
{
    public GraphNodeSO NodeSO { get; private set; }
    public Port output;
    public List<Port> inputs = new List<Port>(); // Track all input ports

    private static readonly Vector2 DefaultSize = new(220, 140);
    private bool initialized;
    private VisualElement inputPortContainer;

    public GraphNode(GraphNodeSO node)
    {
        NodeSO = node;
        if (NodeSO == null)
        {
            Debug.LogError("GraphNode received null NodeSO");
            return;
        }

        title = node.name;

        // Output port (always multi)
        output = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(float));
        output.portName = "Output";
        outputContainer.Add(output);

        // Container for dynamic inputs
        inputPortContainer = new VisualElement();
        inputPortContainer.style.flexDirection = FlexDirection.Column;
        extensionContainer.Add(inputPortContainer);

        // Machine node: add inputs
        if (NodeSO is MachineNodeSO machine)
        {
            // Ensure at least 1 default input
            if (machine.inputs.Count == 0)
            {
                var defaultInput = ScriptableObject.CreateInstance<ResourceNodeSO>();
                defaultInput.name = "Input 1";
                defaultInput.SetResourceName("Input 1");
                machine.inputs.Add(defaultInput);
                AssetDatabase.AddObjectToAsset(defaultInput, machine);
                EditorUtility.SetDirty(machine);
            }

            foreach (var inputNode in machine.inputs)
            {
                if (inputNode != null)
                    AddInputPort(inputNode, machine);
            }

            // Button to add more inputs
            var addInputBtn = new Button(() =>
            {
                var newInput = ScriptableObject.CreateInstance<ResourceNodeSO>();
                newInput.name = "New Input";
                newInput.SetResourceName("New Input");
                machine.inputs.Add(newInput);
                AssetDatabase.AddObjectToAsset(newInput, machine);
                EditorUtility.SetDirty(machine);

                AddInputPort(newInput, machine);
            })
            { text = "+ Input" };
            extensionContainer.Add(addInputBtn);
        }
        else
        {
            // Non-machine nodes: single input
            AddInputPort(null, null);
        }

        RefreshExpandedState();
        RefreshPorts();

        // Restore position
        SetPosition(new Rect(NodeSO.position, DefaultSize));

        // Save position when moved
        RegisterCallback<GeometryChangedEvent>(_ =>
        {
            if (!initialized)
            {
                initialized = true;
                return;
            }

            Vector2 pos = GetPosition().position;
            if (NodeSO.position != pos)
            {
                NodeSO.position = pos;
                EditorUtility.SetDirty(NodeSO);
            }
        });

        // Node color
        if (NodeSO is MachineNodeSO)
            style.backgroundColor = new StyleColor(new Color(1f, 0.9f, 0.2f)); // Yellow
        else if (NodeSO is ResourceNodeSO)
            style.backgroundColor = new StyleColor(new Color(0.2f, 0.6f, 1f)); // Blue
        else
            style.backgroundColor = new StyleColor(new Color(0.5f, 0.5f, 0.5f)); // Gray
    }

    // Add input port with optional remove button for machine nodes
    private void AddInputPort(ResourceNodeSO inputNode, MachineNodeSO machine)
    {
        var port = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(float));
        port.portName = inputNode != null ? inputNode.ResourceName : "Input";

        // Container to hold port + remove button
        var container = new VisualElement();
        container.style.flexDirection = FlexDirection.Row;
        container.Add(port);

        if (machine != null && inputNode != null)
        {
            var removeBtn = new Button(() =>
            {
                // Remove input node from SO
                machine.inputs.Remove(inputNode);
                AssetDatabase.RemoveObjectFromAsset(inputNode);
                EditorUtility.SetDirty(machine);

                // Remove port visually
                inputs.Remove(port);
                inputPortContainer.Remove(container);

                // Remove any connected edges
                var edgesToRemove = port.connections.ToList();
                foreach (var edge in edgesToRemove)
                    edge.input?.Disconnect(edge);
            })
            { text = "X", style = { width = 20 } };

            container.Add(removeBtn);
        }

        inputPortContainer.Add(container);
        inputs.Add(port);

        RefreshExpandedState();
        RefreshPorts();
    }
}
#endif
