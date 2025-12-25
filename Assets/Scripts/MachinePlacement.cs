using System.Collections.Generic;
using UnityEngine;

public class MachinePlacement : MonoBehaviour
{
    [Header("Placement Settings")]
    public LayerMask placementLayer;
    public float placementDistance = 5f;
    public float gridSize = 1f;
    public float rotationStep = 15f;

    [Header("References")]
    public Transform playerCamera;
    public GameObject placementPreviewPrefab;
    public PlayerInventory inventory;

    private GameObject currentPreview;
    private float previewRotationY;
    private bool enable;

    private bool validPlacement = false;
    public bool debug = true;

    private void Start()
    {
        inventory.OnChangeInventoryItem += DestroyPreview;
    }

    private void OnDisable()
    {
        inventory.OnChangeInventoryItem -= DestroyPreview;
    }

    void Update()
    {
        if (!enable) return;

        HandlePreview();
        HandlePlacementInput();
        HandlePreviewRotation();
    }

    public void SetEnablePlacement(bool _enable)
    {
        enable = _enable;
        if (!enable) DestroyPreview();
    }

    public void SetEnablePlacement()
    {
        enable = !enable;
        SetEnablePlacement(enable);
    }

    /* =========================
     * PREVIEW
     * ========================= */
    void HandlePreview()
    {
        var item = inventory.SelectedItem;
        if (item == null)
        {
            DestroyPreview();
            return;
        }

        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        if (!Physics.Raycast(ray, out RaycastHit hit, placementDistance, placementLayer))
        {
            DestroyPreview();
            return;
        }

        Vector3 position = SnapToGrid(hit.point);

        // Snap on top of resource node
        validPlacement = true;
        if (hit.collider.TryGetComponent(out ResourceNode node))
        {
            Collider col = node.GetComponent<Collider>();
            position = col.bounds.center;
            position.y = col.bounds.max.y;

            if (item.machineData.role != MachineRole.Extractor)
                validPlacement = false;
        }
        else
        {
            if (item.machineData.role == MachineRole.Extractor)
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

    void DestroyPreview()
    {
        if (currentPreview != null)
            Destroy(currentPreview);
    }

    /* =========================
     * INPUT
     * ========================= */
    void HandlePlacementInput()
    {
        if (currentPreview == null) return;
        if (!validPlacement) return;

        if (Input.GetMouseButtonDown(0))
        {
            PlaceMachine(currentPreview.transform.position, previewRotationY);
        }
    }

    void HandlePreviewRotation()
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

    /* =========================
     * PLACE
     * ========================= */
    void PlaceMachine(Vector3 position, float rotationY)
    {
        var item = inventory.SelectedItem;
        if (item == null) return;

        if (!debug)
        {
            var costDict = new Dictionary<string, float>();
            foreach (var c in item.cost)
                costDict[c.resourceName] = c.amount;

            if (!inventory.HasResources(costDict))
            {
                Debug.Log("Not enough resources");
                return;
            }

            inventory.ConsumeResources(costDict);
        }

        GameObject machineObj = Instantiate(item.prefab, position, Quaternion.Euler(0, rotationY, 0));
        Machine machine = machineObj.GetComponent<Machine>();
        if (machine != null)
        {
            machine.Initialize(item.machineData);

            // Auto-connect to ResourceNode if this is an extractor
            if (item.machineData.role == MachineRole.Extractor)
            {
                Ray ray = new Ray(playerCamera.position, playerCamera.forward);
                if (Physics.Raycast(ray, out RaycastHit hit, placementDistance, placementLayer))
                {
                    if (hit.collider.TryGetComponent(out ResourceNode node))
                    {
                        node.ConnectMachine(machine);
                        Debug.Log($"{machine.machineData.name} connected to {node.resourceData.ResourceName}");
                    }
                }
            }
        }
    }

    /* =========================
     * HELPERS
     * ========================= */
    Vector3 SnapToGrid(Vector3 position)
    {
        return new Vector3(
            Mathf.Round(position.x / gridSize) * gridSize,
            Mathf.Round(position.y / gridSize) * gridSize,
            Mathf.Round(position.z / gridSize) * gridSize
        );
    }

    void DisablePreviewColliders(GameObject preview)
    {
        foreach (Collider col in preview.GetComponentsInChildren<Collider>())
            col.enabled = false;
    }

    void SetPreviewMaterial(GameObject preview)
    {
        foreach (Renderer r in preview.GetComponentsInChildren<Renderer>())
        {
            Material mat = new Material(r.sharedMaterial);
            mat.color = new Color(1f, 1f, 1f, 0.5f);
            r.material = mat;
        }
    }

    void SetPreviewColor(bool valid)
    {
        if (currentPreview == null) return;

        Color c = valid ? Color.green : Color.red;
        foreach (Renderer r in currentPreview.GetComponentsInChildren<Renderer>())
        {
            Material mat = r.material;
            mat.color = new Color(c.r, c.g, c.b, 0.5f);
            r.material = mat;
        }
    }
}
