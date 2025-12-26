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
    private readonly List<GameObject> currentPreviews = new();
    private GameObject idleBeltPreview;

    [Header("Belt Snapping")]
    public float portSnapRadius = 1.5f;
    private float offsetFromPort = -0.3f;

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
        DestroyAllPreviews();
        enable = !enable;
    }

    private void OnInventoryItemChanged()
    {
        DestroyAllPreviews();
        enable = inventory.SelectedItem != null;
        currentType = inventory.SelectedItem.isBelt ? BuildType.Belt : BuildType.Machine;
    }

    private void Update()
    {
        if (!enable || inventory.SelectedItem == null) return;

        if (currentType == BuildType.Machine)
        {
            HandleMachinePreview();
            HandleRotation();

            if (Input.GetMouseButtonDown(0))
                PlaceMachine();
        }
        else if (currentType == BuildType.Belt)
        {
            // ⭐ NEW: show belt preview BEFORE clicking
            if (!placingBelt)
                HandleIdleBeltPreview();

            if (Input.GetMouseButtonDown(0))
                StartBeltPlacement();

            if (placingBelt)
                HandleBeltPreview();

            if (Input.GetMouseButtonUp(0))
                FinishBeltPlacement();
        }
    }

    public void SetEnablePlacement(bool _enable, BuildType type)
    {
        enable = _enable;
        currentType = type;
        DestroyAllPreviews();
    }

    /* ===================== MACHINE ===================== */
    void HandleMachinePreview()
    {
        var item = inventory.SelectedItem;
        if (item == null) return;

        if (!Physics.Raycast(playerCamera.position, playerCamera.forward, out RaycastHit hit, placementDistance, placementLayer))
        {
            DestroySinglePreview();
            return;
        }

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
        GameObject obj = Instantiate(item.prefab, currentPreview.transform.position, Quaternion.Euler(0, previewRotationY, 0));

        if (obj.TryGetComponent(out Machine machine))
            machine.Initialize(item.machineData);
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
    void HandleIdleBeltPreview()
    {
        if (!Physics.Raycast(
            playerCamera.position,
            playerCamera.forward,
            out RaycastHit hit,
            placementDistance,
            placementLayer))
        {
            DestroyIdleBeltPreview();
            return;
        }

        Vector3 pos = SnapToGrid(hit.point);

        // 🔹 SNAP PRIORITY
        // 1. Machine OUTPUT
        if (TryGetMachinePort(pos, out Transform port, output: true))
        {
            pos = port.position + new Vector3(offsetFromPort, 0, 0);
        }
        // 2. Existing belt
        else if (TrySnapToBelt(pos, out Vector3 beltSnap))
        {
            pos = beltSnap;
        }

        if (idleBeltPreview == null)
        {
            idleBeltPreview = Instantiate(
                inventory.SelectedItem.prefab,
                pos,
                Quaternion.identity
            );

            DisablePreviewColliders(idleBeltPreview);
            SetPreviewTransparency(idleBeltPreview, 0.4f);
        }
        else
        {
            idleBeltPreview.transform.position = pos;
        }
    }


    void StartBeltPlacement()
    {
        if (placingBelt) return;

        if (Physics.Raycast(playerCamera.position, playerCamera.forward,
            out RaycastHit hit, placementDistance, placementLayer))
        {
            Vector3 snapped = SnapToGrid(hit.point);

            // 🔹 SNAP TO MACHINE OUTPUT PORT
            if (TryGetMachinePort(snapped, out Transform port, output: true))
                beltStartPos = port.position + new Vector3(offsetFromPort, 0, 0);
            else
                beltStartPos = snapped;

            placingBelt = true;
        }
    }

    void HandleBeltPreview()
    {
        if (!placingBelt) return;

        if (Physics.Raycast(playerCamera.position, playerCamera.forward,
            out RaycastHit hit, placementDistance, placementLayer))
        {
            Vector3 end = SnapToGrid(hit.point);

            Debug.Log("Raycasting : " + hit.collider.name);
            // 🔹 SNAP TO MACHINE INPUT PORT
            if (TryGetMachinePort(end, out Transform port, output: false))
                end = port.position + new Vector3(offsetFromPort, 0, 0);

            DrawBeltPreview(beltStartPos, end);
        }
    }

    void FinishBeltPlacement()
    {
        if (!placingBelt) return;
        placingBelt = false;

        if (Physics.Raycast(playerCamera.position, playerCamera.forward,
            out RaycastHit hit, placementDistance, placementLayer))
        {
            Vector3 end = SnapToGrid(hit.point);

            if (TryGetMachinePort(end, out Transform port, output: false))
                end = port.position + new Vector3(offsetFromPort, 0, 0);

            PlaceBeltSegments(beltStartPos, end, inventory.SelectedItem);
        }

        DestroyAllPreviews();
    }

    void DrawBeltPreview(Vector3 start, Vector3 end)
    {
        DestroyBeltDragPreviews(); // ❗ CHANGED (not DestroyAll)

        Vector3 dir = end - start;
        if (dir.sqrMagnitude < 0.01f) return;

        dir.Normalize();
        float distance = Vector3.Distance(start, end);
        int segments = Mathf.FloorToInt(distance / beltSegmentLength);

        for (int i = 0; i <= segments; i++)
        {
            Vector3 pos = start + dir * i * beltSegmentLength;
            GameObject preview = Instantiate(
                inventory.SelectedItem.prefab,
                pos,
                Quaternion.LookRotation(dir)
            );

            DisablePreviewColliders(preview);
            SetPreviewTransparency(preview, 0.4f);
            currentPreviews.Add(preview);
        }
    }

    void PlaceBeltSegments(Vector3 start, Vector3 end, BuildItemSO item)
    {
        Vector3 dir = (end - start).normalized;
        float distance = Vector3.Distance(start, end);
        int segments = Mathf.FloorToInt(distance / beltSegmentLength);

        ConveyorBelt previous = null;

        for (int i = 0; i <= segments; i++)
        {
            Vector3 pos = start + dir * i * beltSegmentLength;
            GameObject obj = Instantiate(item.prefab, pos, Quaternion.LookRotation(dir));

            if (!obj.TryGetComponent(out ConveyorBelt belt)) continue;

            // Assign start/end points for visuals
            if (previous != null)
            {
                previous.outputBelt = belt;
                belt.inputBelt = previous;

                previous.startPoint ??= previous.transform; // fallback
                previous.endPoint = belt.transform;
            }
            else
            {
                // First belt: connect to machine output if snapping to port
                if (TryGetMachinePort(start, out Transform port, output: true))
                {
                    Machine sourceMachine = port.GetComponentInParent<Machine>();
                    if (sourceMachine != null)
                    {
                        belt.ConnectToMachine(sourceMachine, asInput: false);
                        sourceMachine.ConnectBelt(belt);
                        belt.startPoint = port;
                    }
                    else
                    {
                        belt.startPoint = belt.transform;
                    }
                }
                else
                {
                    belt.startPoint = belt.transform;
                }
            }

            // Last belt's endPoint points to machine input if snapping
            if (i == segments)
            {
                if (TryGetMachinePort(end, out Transform endPort, output: false))
                {
                    Machine targetMachine = endPort.GetComponentInParent<Machine>();
                    if (targetMachine != null)
                    {
                        belt.ConnectToMachine(targetMachine, asInput: true);
                        targetMachine.ConnectBelt(belt);
                        belt.endPoint = endPort;
                    }
                    else
                    {
                        belt.endPoint = belt.transform;
                    }
                }
                else
                {
                    belt.endPoint = belt.transform;
                }
            }

            previous = belt;
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

    void DestroyIdleBeltPreview()
    {
        if (idleBeltPreview != null)
            Destroy(idleBeltPreview);
        idleBeltPreview = null;
    }

    void DestroyBeltDragPreviews()
    {
        foreach (var p in currentPreviews)
            Destroy(p);
        currentPreviews.Clear();
    }

    void DestroySinglePreview()
    {
        if (currentPreview != null)
            Destroy(currentPreview);
        currentPreview = null;
    }

    void DestroyAllPreviews()
    {
        DestroySinglePreview();
        DestroyIdleBeltPreview();
        DestroyBeltDragPreviews();
    }

    void DisablePreviewColliders(GameObject preview)
    {
        foreach (var col in preview.GetComponentsInChildren<Collider>())
            col.enabled = false;
    }

    void SetPreviewTransparency(GameObject obj, float alpha)
    {
        foreach (Renderer r in obj.GetComponentsInChildren<Renderer>())
        {
            Material m = r.material;
            Color c = m.color;
            c.a = alpha;
            m.color = c;
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
        }
    }

    bool TryGetMachinePort(Vector3 pos, out Transform port, bool output)
    {
        Collider[] hits = Physics.OverlapSphere(pos, portSnapRadius, placementLayer);

        foreach (var h in hits)
        {
            Transform p = null;
            if (h.transform.name == StringConstants.outputPort || h.transform.name == StringConstants.inputPort)
            {
                p = h.transform;
            }

            if (p != null)
            {
                port = p;
                return true;
            }
        }

        port = null;
        return false;
    }

    bool TrySnapToBelt(Vector3 pos, out Vector3 snapPos)
    {
        Collider[] hits = Physics.OverlapSphere(pos, portSnapRadius, placementLayer);

        foreach (var h in hits)
        {
            if (h.TryGetComponent(out ConveyorBelt belt))
            {
                snapPos = belt.transform.position;
                return true;
            }
        }

        snapPos = Vector3.zero;
        return false;
    }
}
