using UnityEngine;

public class MachinePlacement : MonoBehaviour
{
    [Header("Placement Settings")]
    public LayerMask placementLayer;       // Layers you can place machines on
    public float placementDistance = 5f;   // How far from player
    public float gridSize = 1f;            // Grid snapping size

    [Header("References")]
    public Transform playerCamera;
    public GameObject placementPreviewPrefab;  // Ghost machine prefab
    private GameObject currentPreview;

    [Header("Machine Prefabs")]
    public GameObject[] machinePrefabs;    // Prefabs mapped to MachineNodeSO
    private int selectedMachineIndex = 0;

    private bool enable = false;

    void Update()
    {
        if (enable)
        {
            HandlePreview();
            HandlePlacementInput();
            HandleSwitchMachine();
        }
    }

    public void SetEnablePlacement()
    {
        enable = !enable;
        if (!enable && currentPreview != null)
        {
            Destroy(currentPreview);
        }
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
            }
            else
            {
                currentPreview.transform.position = position;
            }
        }
        else
        {
            if (currentPreview != null)
                Destroy(currentPreview);
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
            }
        }
    }

    void PlaceMachine(Vector3 position)
    {
        GameObject prefabToPlace = machinePrefabs[selectedMachineIndex];
        Instantiate(prefabToPlace, position, Quaternion.identity);
    }

    Vector3 SnapToGrid(Vector3 position)
    {
        float x = Mathf.Round(position.x / gridSize) * gridSize;
        float y = Mathf.Round(position.y / gridSize) * gridSize;
        float z = Mathf.Round(position.z / gridSize) * gridSize;
        return new Vector3(x, y, z);
    }
}
