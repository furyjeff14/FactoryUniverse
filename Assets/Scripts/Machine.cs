using System.Collections.Generic;
using UnityEngine;

public class Machine : MonoBehaviour
{
    public MachineNodeSO machineData;
    public float productionRate = 1f; // units/sec
    public GameObject outputItemPrefab;

    [HideInInspector] public Dictionary<string, float> inputBuffers = new();
    [HideInInspector] public Dictionary<string, float> outputBuffers = new();
    private Dictionary<ConveyorBelt, Dictionary<string, float>> beltTransferTimers = new();

    public List<ConveyorBelt> connectedBelts = new List<ConveyorBelt>();
    public List<Machine> connectedMachines = new();

    private bool initialized = false;
    public ResourceNode attachedNode;

    void Start()
    {
        if (machineData != null)
            Initialize(machineData);
    }

    void Update()
    {
        if (!initialized) return;

        switch (machineData.role)
        {
            case MachineRole.Extractor:
                ExtractResources(Time.deltaTime);
                break;

            case MachineRole.Processor:
                ProcessProduction(Time.deltaTime);
                break;
        }

        TransferOutputs();
    }

    public void Initialize(MachineNodeSO data)
    {
        machineData = data;
        inputBuffers.Clear();
        outputBuffers.Clear();

        if (machineData.inputs != null)
            foreach (var input in machineData.inputs)
                inputBuffers[input.ResourceName] = 0f;

        if (machineData.outputs != null)
            foreach (var output in machineData.outputs)
                outputBuffers[output.ResourceName] = 0f;

        if (machineData.role == MachineRole.Extractor)
        {
            FindResourceNode();
            ProcessOutputResource(attachedNode);
        }

        initialized = true;
    }

    void FindResourceNode()
    {
        attachedNode = GetComponentInParent<ResourceNode>() ?? GetComponentInChildren<ResourceNode>();
        if (attachedNode == null)
        {
            ResourceNode[] nodes = FindObjectsOfType<ResourceNode>();
            float closestDist = float.MaxValue;
            foreach (var n in nodes)
            {
                float dist = Vector3.Distance(transform.position, n.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    attachedNode = n;
                }
            }
        }
    }

    private void ProcessOutputResource(ResourceNode _node)
    {
        outputItemPrefab = _node.resourceData.resourcePrefab;
    }

    void ExtractResources(float deltaTime)
    {
        if (attachedNode == null) return;

        foreach (var output in machineData.outputs)
        {
            float amount = machineData.extractionRate * deltaTime;
            attachedNode.Extract(amount, output.ResourceName);

            if (!outputBuffers.ContainsKey(output.ResourceName))
                outputBuffers[output.ResourceName] = 0f;

            outputBuffers[output.ResourceName] += amount;
        }
    }

    void ProcessProduction(float deltaTime)
    {
        bool canProduce = true;
        foreach (var input in machineData.inputs)
        {
            if (!inputBuffers.ContainsKey(input.ResourceName) || inputBuffers[input.ResourceName] <= 0f)
            {
                canProduce = false;
                break;
            }
        }

        if (!canProduce) return;

        float amountProduced = productionRate * deltaTime;

        foreach (var input in machineData.inputs)
            inputBuffers[input.ResourceName] -= amountProduced;

        foreach (var output in machineData.outputs)
        {
            if (!outputBuffers.ContainsKey(output.ResourceName))
                outputBuffers[output.ResourceName] = 0f;

            outputBuffers[output.ResourceName] += amountProduced;
        }
    }

    void TransferOutputs()
    {
        // Send outputs to connected machines (no cap)
        foreach (var machine in connectedMachines)
        {
            foreach (var resource in new List<string>(outputBuffers.Keys))
            {
                if (outputBuffers[resource] <= 0f) continue;
                if (machine.inputBuffers.ContainsKey(resource))
                {
                    machine.inputBuffers[resource] += outputBuffers[resource];
                    outputBuffers[resource] = 0f;
                }
            }
        }

        // Send outputs to belts 1 by 1, rate based on belt speed
        foreach (var belt in connectedBelts)
        {
            if (belt == null) continue;

            foreach (var resource in new List<string>(outputBuffers.Keys))
            {
                if (outputBuffers[resource] <= 0f) continue;

                if (outputItemPrefab != null)
                    belt.itemPrefab = outputItemPrefab;

                float unitsPerSecond = belt.speed; // 1 unit per belt speed per second

                // Track leftover fractional units
                if (!beltTransferTimers.ContainsKey(belt))
                    beltTransferTimers[belt] = new Dictionary<string, float>();

                if (!beltTransferTimers[belt].ContainsKey(resource))
                    beltTransferTimers[belt][resource] = 0f;

                beltTransferTimers[belt][resource] += Time.deltaTime * unitsPerSecond;

                int unitsToSend = Mathf.FloorToInt(beltTransferTimers[belt][resource]);
                beltTransferTimers[belt][resource] -= unitsToSend;

                for (int i = 0; i < unitsToSend; i++)
                {
                    if (outputBuffers[resource] <= 0f) break;

                    // Check if belt can accept another visual
                    float availableSpace = belt.GetAvailableVisualSpace();
                    if (availableSpace <= 0f) break;

                    belt.ReceiveResource(resource, 1f); // send 1 unit at a time
                    outputBuffers[resource] -= 1f;
                }
            }
        }
    }

    public void ConnectMachine(Machine machine)
    {
        if (!connectedMachines.Contains(machine))
            connectedMachines.Add(machine);
    }

    public void ConnectBelt(ConveyorBelt belt)
    {
        if (!connectedBelts.Contains(belt))
            connectedBelts.Add(belt);
    }
}
