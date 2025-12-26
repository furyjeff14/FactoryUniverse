using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class ConveyorBelt : MonoBehaviour
{
    [Header("Belt Settings")]
    public float speed = 2f;
    private float beltHeightOffset = 0.97f;

    [Header("Connections")]
    public Machine inputMachine;
    public Machine outputMachine;
    public ConveyorBelt inputBelt;
    public ConveyorBelt outputBelt;

    [HideInInspector]
    public Dictionary<string, float> buffer = new Dictionary<string, float>();

    [Header("Visuals")]
    public Transform startPoint;
    public Transform endPoint;
    public GameObject itemPrefab;

    private List<GameObject> visualItems = new List<GameObject>();
    private List<string> visualResourceType = new List<string>();
    private List<float> visualPositions = new List<float>();

    // For moving objects above the belt
    private HashSet<Rigidbody> objectsOnBelt = new HashSet<Rigidbody>();
    private Vector3 beltDirection;

    void Start()
    {
        AutoAssignPoints();

        // Setup collider
        BoxCollider col = GetComponent<BoxCollider>();
        col.isTrigger = true;

        // Precompute belt direction
        if (startPoint != null && endPoint != null)
            beltDirection = (endPoint.position - startPoint.position).normalized;
    }

    void Update()
    {
        MoveResources(Time.deltaTime);
        MoveObjectsOnBelt(Time.deltaTime);
    }

    void AutoAssignPoints()
    {
        if (inputMachine != null)
        {
            Transform port = inputMachine.transform.Find("outputPort");
            if (port != null) startPoint = port;
        }
        else if (inputBelt != null) startPoint = inputBelt.endPoint;

        if (outputMachine != null)
        {
            Transform port = outputMachine.transform.Find("inputPort");
            if (port != null) endPoint = port;
        }
        else if (outputBelt != null) endPoint = outputBelt.startPoint;

        if (startPoint == null) startPoint = transform;
        if (endPoint == null) endPoint = transform;

        beltDirection = (endPoint.position - startPoint.position).normalized;
    }

    void MoveResources(float deltaTime)
    {
        if (startPoint == null || endPoint == null) return;

        float distance = Vector3.Distance(startPoint.position, endPoint.position);

        for (int i = visualItems.Count - 1; i >= 0; i--)
        {
            float spacing = 0.2f / distance;
            if (i > 0 && visualPositions[i] + spacing > visualPositions[i - 1])
                continue;

            visualPositions[i] += speed * deltaTime / distance;

            if (visualPositions[i] >= 1f)
            {
                if (outputBelt != null && outputBelt.HasVisualSpace())
                {
                    outputBelt.TransferVisual(visualItems[i], visualResourceType[i]);
                    visualItems.RemoveAt(i);
                    visualPositions.RemoveAt(i);
                    visualResourceType.RemoveAt(i);
                }
                else
                {
                    visualPositions[i] = 1f;
                    visualItems[i].transform.position = endPoint.position + Vector3.up * beltHeightOffset;
                    visualItems[i].transform.rotation = Quaternion.LookRotation(beltDirection);
                }
            }
            else
            {
                Vector3 basePos = Vector3.Lerp(startPoint.position, endPoint.position, visualPositions[i]);
                visualItems[i].transform.position = basePos + Vector3.up * beltHeightOffset;
                visualItems[i].transform.rotation = Quaternion.LookRotation(beltDirection);
            }
        }

        // Transfer buffers to machine
        if (outputMachine != null)
        {
            foreach (var key in new List<string>(buffer.Keys))
            {
                if (buffer[key] <= 0f) continue;
                if (!outputMachine.inputBuffers.ContainsKey(key)) continue;

                float amount = Mathf.Min(buffer[key], speed * deltaTime);
                outputMachine.inputBuffers[key] += amount;
                buffer[key] -= amount;
            }
        }

        // Transfer buffers to next belt
        if (outputBelt != null)
        {
            foreach (var key in new List<string>(buffer.Keys))
            {
                if (buffer[key] <= 0f) continue;
                float availableSpace = outputBelt.GetAvailableVisualSpace();
                if (availableSpace <= 0f) continue;

                float amount = Mathf.Min(buffer[key], speed * deltaTime, availableSpace);
                outputBelt.ReceiveResource(key, amount);
                buffer[key] -= amount;
            }
        }

        // Clean empty buffer entries
        var keysToRemove = new List<string>();
        foreach (var k in buffer.Keys)
            if (buffer[k] <= 0f) keysToRemove.Add(k);

        foreach (var k in keysToRemove) buffer.Remove(k);
    }

    void MoveObjectsOnBelt(float deltaTime)
    {
        Vector3 movement = beltDirection * speed * deltaTime;
        foreach (var rb in objectsOnBelt)
        {
            if (rb != null)
                rb.velocity = new Vector3(movement.x, rb.velocity.y, movement.z);
        }
    }

    // Trigger detection
    private void OnTriggerEnter(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;
        if (rb != null && !objectsOnBelt.Contains(rb))
            objectsOnBelt.Add(rb);
    }

    private void OnTriggerExit(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;
        if (rb != null && objectsOnBelt.Contains(rb))
            objectsOnBelt.Remove(rb);
    }

    public void TransferVisual(GameObject visual, string resourceName)
    {
        if (startPoint == null) return;
        visual.transform.position = startPoint.position + Vector3.up * beltHeightOffset;
        visualItems.Add(visual);
        visualResourceType.Add(resourceName);
        visualPositions.Add(0.001f);
    }

    public bool HasVisualSpace()
    {
        float distance = Vector3.Distance(startPoint.position, endPoint.position);
        float spacing = 0.2f / distance;
        if (visualItems.Count == 0) return true;
        float lastPos = visualPositions[visualItems.Count - 1];
        return lastPos + spacing < 1f;
    }

    public float GetAvailableVisualSpace()
    {
        if (visualItems.Count == 0) return Mathf.Infinity;
        float lastPos = visualPositions[visualItems.Count - 1];
        float distance = Vector3.Distance(startPoint.position, endPoint.position);
        float spacing = 0.2f / distance;
        return Mathf.Max(0f, 1f - lastPos - spacing);
    }

    public void ReceiveResource(string resourceName, float amount)
    {
        if (!buffer.ContainsKey(resourceName)) buffer[resourceName] = 0f;
        buffer[resourceName] += amount;

        if (itemPrefab != null)
        {
            int visualCount = Mathf.Max(1, Mathf.RoundToInt(amount));
            for (int i = 0; i < visualCount; i++)
                SpawnVisual(resourceName);
        }
    }

    void SpawnVisual(string resourceName)
    {
        if (startPoint == null || itemPrefab == null) return;
        Vector3 spawnPos = startPoint.position + Vector3.up * beltHeightOffset;
        GameObject obj = Instantiate(itemPrefab, spawnPos, Quaternion.identity);
        obj.name = resourceName;

        visualItems.Add(obj);
        visualResourceType.Add(resourceName);
        visualPositions.Add(0.001f);
    }

    public void ConnectToMachine(Machine machine, bool asInput = false)
    {
        if (asInput) inputMachine = machine;
        else outputMachine = machine;
        AutoAssignPoints();
    }

    public void ConnectToBelt(ConveyorBelt belt, bool asInput = false)
    {
        if (asInput) inputBelt = belt;
        else outputBelt = belt;
        AutoAssignPoints();
    }
}
