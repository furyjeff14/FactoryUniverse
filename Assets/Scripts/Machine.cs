using System.Collections.Generic;
using UnityEngine;

public class Machine : MonoBehaviour
{
    [Header("Machine Data")]
    public MachineNodeSO machineData;

    [Header("Runtime Buffers")]
    public Dictionary<string, float> inputBuffers = new Dictionary<string, float>();
    public Dictionary<string, float> outputBuffers = new Dictionary<string, float>();

    [Header("Settings")]
    public float productionRate = 1f; // units per second

    [Header("Connections")]
    public List<Machine> connectedMachines = new List<Machine>();

    void Start()
    {
        // Initialize buffers from machineData
        foreach (var input in machineData.inputs)
            inputBuffers[input.ResourceName] = 0f;

        foreach (var output in machineData.outputs)
            outputBuffers[output.ResourceName] = 0f;
    }

    void Update()
    {
        ProcessProduction(Time.deltaTime);
        TransferOutputs();
    }

    void ProcessProduction(float deltaTime)
    {
        // Example: produce outputs if inputs are sufficient
        bool canProduce = true;
        foreach (var input in machineData.inputs)
        {
            if (inputBuffers[input.ResourceName] <= 0f)
                canProduce = false;
        }

        if (!canProduce) return;

        // Consume inputs
        foreach (var input in machineData.inputs)
            inputBuffers[input.ResourceName] -= productionRate * deltaTime;

        // Produce outputs
        foreach (var output in machineData.outputs)
            outputBuffers[output.ResourceName] += productionRate * deltaTime;
    }

    void TransferOutputs()
    {
        foreach (var machine in connectedMachines)
        {
            foreach (var resource in outputBuffers.Keys)
            {
                float available = outputBuffers[resource];
                if (available <= 0f) continue;

                // Transfer to connected machine input buffer
                if (machine.inputBuffers.ContainsKey(resource))
                {
                    float transferAmount = Mathf.Min(available, productionRate * Time.deltaTime);
                    machine.inputBuffers[resource] += transferAmount;
                    outputBuffers[resource] -= transferAmount;
                }
            }
        }
    }

    // Add a machine connection
    public void ConnectMachine(Machine other)
    {
        if (!connectedMachines.Contains(other))
            connectedMachines.Add(other);
    }
}
