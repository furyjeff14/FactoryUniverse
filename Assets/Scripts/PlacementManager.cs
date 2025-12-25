using System.Collections.Generic;
using UnityEngine;

public enum BuildType { Machine, Belt }

public class PlacementManager : MonoBehaviour
{
    [Header("References")]
    public Transform playerCamera;
    public PlayerInventory inventory;

    [Header("Placement Settings")]
    public LayerMask placementLayer;
    public float placementDistance = 5f;
    public float gridSize = 1f;
    public float rotationStep = 15f;
    public float beltSegmentLength = 1f;

    private GameObject currentPreview;
    private float previewRotationY;
    private bool enable;
    private bool validPlacement = true;
    private BuildType currentType = BuildType.Machine;

    // Belt placement
    private bool placingBelt = false;
    private Vector3 beltStartPos;

    private void OnEnable()
    {
        if (inventory != null)
        {
            inventory.OnChangeInventoryItem += OnInventoryItemChanged;
            inventory.OnToggleInventoryItem += OnToggleInventoryItem;
        }
    }

    private void OnDisable()
    {
        if (inventory != null)
        {
            inventory.OnChangeInventoryItem -= OnInventoryItemChanged;
            inventory.OnToggleInventoryItem -= OnToggleInventoryItem;
        }
    }

    private void OnToggleInventoryItem()
    {
        DestroyPreview();
        enable = false;
    }

    private void OnInventoryItemChanged()
    {
        DestroyPreview();
        enable = inventory.SelectedItem != null; // Enable placement if item selected
    }

    private void Update()
    {
        if (!enable) return;

        // Handle preview and input based on type
        if (currentType == BuildType.Machine)
        {
            HandleMachinePreview();
            HandleRotation();

            if (Input.GetMouseButtonDown(0))
                PlaceMachine();
        }
        else if (currentType == BuildType.Belt)
        {
            HandleBeltPreview();

            if (Input.GetMouseButtonDown(0))
                StartBeltPlacement();
            if (Input.GetMouseButtonUp(0))
                FinishBeltPlacement();
        }
    }

    public void SetEnablePlacement(bool _enable, BuildType type)
    {
        enable = _enable;
        currentType = type;
        DestroyPreview();
    }

    /* ===================== MACHINE ===================== */
    void HandleMachinePreview()
    {
        var item = inventory.SelectedItem;
        if (item == null) { DestroyPreview(); return; }

        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        if (!Physics.Raycast(ray, out RaycastHit hit, placementDistance, placementLayer)) { DestroyPreview(); return; }

        Vector3 position = SnapToGrid(hit.point);
        validPlacement = true;

        if (hit.collider.TryGetComponent(out ResourceNode node))
        {
            Collider col = node.GetComponent<Collider>();
            position = col.bounds.center;
            position.y = col.bounds.max.y;

            if (item.machineData.role != MachineRole.Extractor)
                validPlacement = false;
        }
        else if (item.machineData.role == MachineRole.Extractor)
        {
            validPlacement = false;
        }

        if (currentPreview == null)
        {
            currentPreview = Instantiate(item.prefab, position, Quaternion.Euler(0, previewRotationY, 0));
            DisablePreviewColliders(currentPreview);
        }
        else
        {
            currentPreview.transform.SetPositionAndRotation(position, Quaternion.Euler(0, previewRotationY, 0));
        }

        SetPreviewColor(validPlacement);
    }

    void PlaceMachine()
    {
        if (!validPlacement || currentPreview == null) return;

        var item = inventory.SelectedItem;
        if (item == null) return;

        GameObject obj = Instantiate(item.prefab, currentPreview.transform.position, Quaternion.Euler(0, previewRotationY, 0));
        Machine machine = obj.GetComponent<Machine>();
        if (machine != null)
            machine.Initialize(item.machineData);

        // Auto-connect to resource node if extractor
        if (item.machineData.role == MachineRole.Extractor)
        {
            Collider[] hits = Physics.OverlapSphere(obj.transform.position, 2f, placementLayer);
            foreach (var hit in hits)
                if (hit.TryGetComponent<ResourceNode>(out var node))
                    node.ConnectMachine(machine);
        }
    }

    void HandleRotation()
    {
        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) > 0.01f)
        {
            previewRotationY += scroll * rotationStep;
            previewRotationY = Mathf.Round(previewRotationY / rotationStep) * rotationStep;
            if (currentPreview != null)
                currentPreview.transform.rotation = Quaternion.Euler(0, previewRotationY, 0);
        }
    }

    /* ===================== BELT ===================== */
    void HandleBeltPreview()
    {
        if (!placingBelt) return;

        if (Physics.Raycast(playerCamera.position, playerCamera.forward, out RaycastHit hit, placementDistance, placementLayer))
        {
            Vector3 endPos = SnapToGrid(hit.point);
            DrawBeltPreview(beltStartPos, endPos);
        }
    }

    void StartBeltPlacement()
    {
        if (placingBelt) return;
        if (Physics.Raycast(playerCamera.position, playerCamera.forward, out RaycastHit hit, placementDistance, placementLayer))
        {
            beltStartPos = SnapToGrid(hit.point);
            placingBelt = true;
        }
    }

    void FinishBeltPlacement()
    {
        if (!placingBelt) return;
        if (Physics.Raycast(playerCamera.position, playerCamera.forward, out RaycastHit hit, placementDistance, placementLayer))
        {
            Vector3 endPos = SnapToGrid(hit.point);
            PlaceBeltSegments(beltStartPos, endPos, inventory.SelectedItem);
        }
        placingBelt = false;
        DestroyPreview();
    }

    void DrawBeltPreview(Vector3 start, Vector3 end)
    {
        DestroyPreview();

        Vector3 dir = (end - start).normalized;
        float distance = Vector3.Distance(start, end);
        int segments = Mathf.CeilToInt(distance / beltSegmentLength);

        for (int i = 0; i < segments; i++)
        {
            Vector3 pos = start + dir * i * beltSegmentLength;
            currentPreview = GameObject.CreatePrimitive(PrimitiveType.Cube); // placeholder
            currentPreview.transform.position = pos + Vector3.up * 0.1f;
            currentPreview.transform.localScale = new Vector3(0.3f, 0.1f, beltSegmentLength);
            currentPreview.GetComponent<Collider>().enabled = false;
        }
    }

    void PlaceBeltSegments(Vector3 start, Vector3 end, BuildItemSO item)
    {
        Vector3 dir = (end - start).normalized;
        float distance = Vector3.Distance(start, end);
        int segments = Mathf.CeilToInt(distance / beltSegmentLength);

        ConveyorBelt previousBelt = null;

        for (int i = 0; i < segments; i++)
        {
            Vector3 pos = start + dir * i * beltSegmentLength;
            GameObject obj = Instantiate(item.prefab, pos, Quaternion.LookRotation(dir));
            ConveyorBelt belt = obj.GetComponent<ConveyorBelt>();
            if (belt != null)
            {
                belt.speed = item.beltSpeed;
                belt.capacity = item.beltCapacity;

                if (previousBelt != null)
                    previousBelt.outputBelt = belt;

                previousBelt = belt;

                // Connect to nearby machines
                Collider[] hits = Physics.OverlapSphere(pos, 1.5f, placementLayer);
                foreach (var hit in hits)
                {
                    if (hit.TryGetComponent<Machine>(out var machine))
                        belt.ConnectToMachine(machine);
                }
            }
        }
    }

    /* ===================== HELPERS ===================== */
    Vector3 SnapToGrid(Vector3 pos)
    {
        return new Vector3(
            Mathf.Round(pos.x / gridSize) * gridSize,
            Mathf.Round(pos.y / gridSize) * gridSize,
            Mathf.Round(pos.z / gridSize) * gridSize
        );
    }

    void DestroyPreview()
    {
        if (currentPreview != null)
        {
            Destroy(currentPreview);
            currentPreview = null;
        }
    }

    void DisablePreviewColliders(GameObject preview)
    {
        foreach (var col in preview.GetComponentsInChildren<Collider>())
            col.enabled = false;
    }

    void SetPreviewColor(bool valid)
    {
        if (currentPreview == null) return;

        Color c = valid ? Color.green : Color.red;
        foreach (Renderer r in currentPreview.GetComponentsInChildren<Renderer>())
        {
            Material mat = r.material;
            mat.color = new Color(c.r, c.g, c.b, 0.5f);
        }
    }
}
