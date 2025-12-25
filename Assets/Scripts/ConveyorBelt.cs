using System.Collections.Generic;
using UnityEngine;

public class ConveyorBelt : MonoBehaviour
{
    [Header("Belt Settings")]
    public float speed = 2f;                   // Units per second
    public float capacity = 50f;               // Max resource amount on belt

    [Header("Connections")]
    public Machine inputMachine;
    public Machine outputMachine;
    public ConveyorBelt inputBelt;
    public ConveyorBelt outputBelt;

    [HideInInspector]
    public Dictionary<string, float> buffer = new Dictionary<string, float>();

    [Header("Visuals (Optional)")]
    public Transform startPoint;
    public Transform endPoint;
    public GameObject itemPrefab;              // small visual cube for items
    private List<GameObject> visualItems = new List<GameObject>();
    private List<float> visualPositions = new List<float>();

    void Update()
    {
        MoveResources(Time.deltaTime);
        UpdateVisuals(Time.deltaTime);
    }

    // Transfers resources along the belt logic
    void MoveResources(float deltaTime)
    {
        if (buffer.Count == 0) return;

        // Transfer to output machine
        if (outputMachine != null)
        {
            foreach (var key in new List<string>(buffer.Keys))
            {
                float amount = Mathf.Min(buffer[key], speed * deltaTime);
                if (outputMachine.inputBuffers.ContainsKey(key))
                {
                    outputMachine.inputBuffers[key] += amount;
                    buffer[key] -= amount;
                }
            }
        }

        // Transfer to next belt
        if (outputBelt != null)
        {
            foreach (var key in new List<string>(buffer.Keys))
            {
                float amount = Mathf.Min(buffer[key], speed * deltaTime);
                if (!outputBelt.buffer.ContainsKey(key))
                    outputBelt.buffer[key] = 0f;

                outputBelt.buffer[key] += amount;
                buffer[key] -= amount;
            }
        }

        // Remove empty entries
        var keysToRemove = new List<string>();
        foreach (var k in buffer.Keys)
            if (buffer[k] <= 0f)
                keysToRemove.Add(k);

        foreach (var k in keysToRemove)
            buffer.Remove(k);
    }

    // Call this to send resource onto the belt
    public void ReceiveResource(string resourceName, float amount)
    {
        if (!buffer.ContainsKey(resourceName))
            buffer[resourceName] = 0f;

        buffer[resourceName] += amount;
        if (buffer[resourceName] > capacity)
            buffer[resourceName] = capacity;

        // Optional: spawn visual item
        if (itemPrefab != null && startPoint != null && endPoint != null)
        {
            GameObject obj = Instantiate(itemPrefab, startPoint.position, Quaternion.identity);
            obj.name = resourceName;
            visualItems.Add(obj);
            visualPositions.Add(0f);
        }
    }

    // Smoothly move visual items along belt
    void UpdateVisuals(float deltaTime)
    {
        if (visualItems.Count == 0 || startPoint == null || endPoint == null) return;

        for (int i = visualItems.Count - 1; i >= 0; i--)
        {
            visualPositions[i] += speed * deltaTime / Vector3.Distance(startPoint.position, endPoint.position);

            if (visualPositions[i] >= 1f)
            {
                Destroy(visualItems[i]);
                visualItems.RemoveAt(i);
                visualPositions.RemoveAt(i);
                continue;
            }

            visualItems[i].transform.position = Vector3.Lerp(startPoint.position, endPoint.position, visualPositions[i]);
        }
    }

    // Optional: auto-connect to machine
    public void ConnectToMachine(Machine machine, bool asInput = false)
    {
        if (asInput)
            inputMachine = machine;
        else
            outputMachine = machine;
    }

    // Optional: auto-connect to another belt
    public void ConnectToBelt(ConveyorBelt belt, bool asInput = false)
    {
        if (asInput)
            inputBelt = belt;
        else
            outputBelt = belt;
    }
}
