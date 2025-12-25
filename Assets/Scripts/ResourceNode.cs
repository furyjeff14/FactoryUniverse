using System.Collections.Generic;
using UnityEngine;

public class ResourceNode : MonoBehaviour
{
    public ResourceNodeSO resourceData;
    public float generationRate = 1f;  // units/sec
    public int maxBuffer = 100;

    [HideInInspector] public float currentBuffer = 0f;

    [Header("Connections")]
    public List<Machine> connectedMachines = new();

    void Update()
    {
        GenerateResource(Time.deltaTime);
        PushResourcesToMachines();
    }

    void GenerateResource(float deltaTime)
    {
        if (currentBuffer >= maxBuffer) return;

        currentBuffer += generationRate * deltaTime;
        if (currentBuffer > maxBuffer) currentBuffer = maxBuffer;
    }

    void PushResourcesToMachines()
    {
        foreach (var machine in connectedMachines)
        {
            if (currentBuffer <= 0f) break;

            if (machine.inputBuffers.ContainsKey(resourceData.ResourceName))
            {
                float amount = Mathf.Min(currentBuffer, machine.productionRate * Time.deltaTime);
                machine.inputBuffers[resourceData.ResourceName] += amount;
                currentBuffer -= amount;
            }
        }
    }

    /// <summary>
    /// Called by player or extractors to remove resource from node
    /// </summary>
    public void Extract(float amount, string resourceName)
    {
        if (resourceData.ResourceName != resourceName) return;

        float extracted = Mathf.Min(currentBuffer, amount);
        currentBuffer -= extracted;
    }

    /// <summary>
    /// Player can manually collect resources
    /// </summary>
    public float CollectResource(float amount)
    {
        float collected = Mathf.Min(currentBuffer, amount);
        currentBuffer -= collected;
        return collected;
    }

    public void ConnectMachine(Machine machine)
    {
        if (!connectedMachines.Contains(machine))
            connectedMachines.Add(machine);
    }
}
