using System.Collections.Generic;
using UnityEngine;

public class MachinePlacementManager : MonoBehaviour
{
    [Header("Placement Settings")]
    public LayerMask placementLayer;
    public float placementDistance = 5f;
    public float gridSize = 1f;

    [Header("References")]
    public Transform playerCamera;
    public GameObject placementPreviewPrefab;

    [Header("Machine Prefabs")]
    public MachineNodeSO[] machineDataList; // Map to prefabs
    public GameObject[] machinePrefabs;     // Prefabs matching machineDataList

    private GameObject currentPreview;
    private int selectedMachineIndex = 0;

    private List<Machine> placedMachines = new List<Machine>();

    void Update()
    {
        HandlePreview();
        HandlePlacementInput();
        HandleSwitchMachine();
    }

    void HandlePreview()
    {
        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, placementDistance, placementLayer))
        {
            Vector3 position = SnapToGrid(hit.point);

            if (currentPreview == null)
            {
                currentPreview = Instantiate(placementPreviewPrefab, position, Quaternion.identity);
                currentPreview.GetComponent<Renderer>().material.color = new Color(1, 1, 1, 0.5f);
            }
            else
            {
                currentPreview.transform.position = position;
            }
        }
        else
        {
            if (currentPreview != null)
            {
                Destroy(currentPreview);
                currentPreview = null;
            }
        }
    }

    void HandlePlacementInput()
    {
        if (currentPreview != null && Input.GetMouseButtonDown(0)) // Left click
        {
            PlaceMachine(currentPreview.transform.position);
        }
    }

    void HandleSwitchMachine()
    {
        if (Input.mouseScrollDelta.y != 0)
        {
            selectedMachineIndex += (int)Input.mouseScrollDelta.y;
            if (selectedMachineIndex < 0) selectedMachineIndex = machinePrefabs.Length - 1;
            if (selectedMachineIndex >= machinePrefabs.Length) selectedMachineIndex = 0;

            if (currentPreview != null)
            {
                Destroy(currentPreview);
                currentPreview = Instantiate(placementPreviewPrefab, currentPreview.transform.position, Quaternion.identity);
                currentPreview.GetComponent<Renderer>().material.color = new Color(1, 1, 1, 0.5f);
            }
        }
    }

    void PlaceMachine(Vector3 position)
    {
        GameObject prefabToPlace = machinePrefabs[selectedMachineIndex];
        MachineNodeSO data = machineDataList[selectedMachineIndex];

        GameObject newMachineObj = Instantiate(prefabToPlace, position, Quaternion.identity);
        Machine machine = newMachineObj.GetComponent<Machine>();
        machine.machineData = data;

        placedMachines.Add(machine);

        // Optionally connect to nearest previous machine automatically
        if (placedMachines.Count > 1)
        {
            Machine previous = placedMachines[placedMachines.Count - 2];
            previous.ConnectMachine(machine);
        }
    }

    Vector3 SnapToGrid(Vector3 position)
    {
        float x = Mathf.Round(position.x / gridSize) * gridSize;
        float y = Mathf.Round(position.y / gridSize) * gridSize;
        float z = Mathf.Round(position.z / gridSize) * gridSize;
        return new Vector3(x, y, z);
    }
}
