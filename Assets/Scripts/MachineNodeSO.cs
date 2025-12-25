using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Factory/Machine Node")]
public class MachineNodeSO : GraphNodeSO
{
    [Header("Machine Info")]
    public string machineName;

    [Header("Role")]
    public MachineRole role;

    [Header("Resources")]
    public List<ResourceNodeSO> inputs = new();
    public List<ResourceNodeSO> outputs = new();

    [Header("Extractor Settings")]
    public float extractionRate = 1f; // units/sec (only used if Extractor)

    [Header("Processor Settings")]
    public float processTime = 1f; // seconds per craft
}

public enum MachineRole
{
    Extractor,
    Processor
}