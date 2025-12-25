using System.Collections.Generic;
using UnityEngine;

public class Machine : MonoBehaviour
{
    public MachineNodeSO machineData;
    public float productionRate = 1f; // units/sec

    [HideInInspector] public Dictionary<string, float> inputBuffers = new();
    [HideInInspector] public Dictionary<string, float> outputBuffers = new();

    public List<Machine> connectedMachines = new();

    private bool initialized = false;
    private ResourceNode attachedNode; // for extractors

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
                TransferOutputs();
                break;
        }
    }

    public void Initialize(MachineNodeSO data)
    {
        machineData = data;
        inputBuffers.Clear();
        outputBuffers.Clear();

        if (machineData.inputs != null)
        {
            foreach (var input in machineData.inputs)
                inputBuffers[input.ResourceName] = 0f;
        }

        if (machineData.outputs != null)
        {
            foreach (var output in machineData.outputs)
                outputBuffers[output.ResourceName] = 0f;
        }

        if (machineData.role == MachineRole.Extractor)
            FindResourceNode();

        initialized = true;
    }

    void FindResourceNode()
    {
        attachedNode = GetComponentInParent<ResourceNode>()
                       ?? GetComponentInChildren<ResourceNode>();

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

    void ExtractResources(float deltaTime)
    {
        if (attachedNode == null) return;

        foreach (var output in machineData.outputs)
        {
            float amount = machineData.extractionRate * deltaTime;
            attachedNode.Extract(amount, output.ResourceName);
            outputBuffers[output.ResourceName] += amount;
        }

        TransferOutputs();
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

        foreach (var input in machineData.inputs)
            inputBuffers[input.ResourceName] -= productionRate * deltaTime;

        foreach (var output in machineData.outputs)
            outputBuffers[output.ResourceName] += productionRate * deltaTime;
    }

    void TransferOutputs()
    {
        foreach (var machine in connectedMachines)
        {
            foreach (var resource in outputBuffers.Keys)
            {
                if (outputBuffers[resource] <= 0f) continue;

                if (machine.inputBuffers.ContainsKey(resource))
                {
                    float amount = Mathf.Min(outputBuffers[resource], productionRate * Time.deltaTime);
                    machine.inputBuffers[resource] += amount;
                    outputBuffers[resource] -= amount;
                }
            }
        }
    }

    public void ConnectMachine(Machine machine)
    {
        if (!connectedMachines.Contains(machine))
            connectedMachines.Add(machine);
    }
}
