using UnityEngine;
using System.Collections.Generic;

public class FactoryRuntimeLoader : MonoBehaviour
{
    public FactoryGraphSO graph;
    public GameObject machinePrefab;

    private Dictionary<string, GameObject> spawned = new();

    void Start()
    {
    }
}
