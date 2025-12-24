using System.Collections.Generic;
using UnityEngine;

public class ConveyorBelt : MonoBehaviour
{
    public Queue<ResourceNodeSO> items = new();
    public int capacity = 10;

    public bool CanAccept => items.Count < capacity;

    public void Add(ResourceNodeSO resource)
    {
        if (CanAccept)
            items.Enqueue(resource);
    }

    public ResourceNodeSO Remove()
    {
        return items.Count > 0 ? items.Dequeue() : null;
    }
}
