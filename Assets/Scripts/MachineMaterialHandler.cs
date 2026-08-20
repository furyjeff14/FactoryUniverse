using System;
using UnityEngine;

[Serializable]
public class MachineMaterialHandler
{
    public GameObject chute;
    public bool isInUse = false;
    public static void HandleMaterialTransfer(Machine machine, float deltaTime)
    {
        if (machine.machineData.role == MachineRole.Extractor)
        {
            //machine.ExtractResources(deltaTime);
        }
        else if (machine.machineData.role == MachineRole.Processor)
        {
            //machine.ProcessProduction(deltaTime);
        }
        //machine.TransferOutputs();
    }
}
