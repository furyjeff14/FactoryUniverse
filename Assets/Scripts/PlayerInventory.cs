using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    /* =======================
       RESOURCES
    ======================= */
    private Dictionary<string, float> resources = new();
    public Action OnChangeInventoryItem;
    public Action OnToggleInventoryItem;

    public void AddResource(string resourceName, float amount)
    {
        if (!resources.ContainsKey(resourceName))
            resources[resourceName] = 0f;

        resources[resourceName] += amount;
    }

    public float GetResourceAmount(string resourceName)
        => resources.TryGetValue(resourceName, out float v) ? v : 0f;

    public bool HasResources(Dictionary<string, float> cost)
    {
        foreach (var c in cost)
            if (GetResourceAmount(c.Key) < c.Value)
                return false;
        return true;
    }

    public void ConsumeResources(Dictionary<string, float> cost)
    {
        foreach (var c in cost)
            resources[c.Key] -= c.Value;
    }

    /* =======================
       BUILD INVENTORY (HOTBAR)
    ======================= */
    [Header("Build Inventory (Hotbar)")]
    public List<BuildItemSO> buildItems = new List<BuildItemSO>();
    public int selectedIndex = 0;

    public BuildItemSO SelectedItem
    {
        get
        {
            if (buildItems.Count == 0) return null;
            selectedIndex = Mathf.Clamp(selectedIndex, 0, buildItems.Count - 1);
            return buildItems[selectedIndex];
        }
    }

    void Update()
    {
        HandleNumberKeys();
        // Optional: Enable scroll switching
        // HandleScrollWheel();
    }

    void HandleNumberKeys()
    {
        int max = Mathf.Min(buildItems.Count, 9); // hotbar keys 1-9

        for (int i = 0; i < max; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                if (selectedIndex != i)
                {
                    selectedIndex = i;
                    OnChangeInventoryItem?.Invoke();
                }
                else
                {
                    // Toggle placement or refresh preview
                    OnToggleInventoryItem?.Invoke();
                }
            }
        }
    }

    // Optional: Hotbar scroll selection
    void HandleScrollWheel()
    {
        if (buildItems.Count == 0 || Mathf.Abs(Input.mouseScrollDelta.y) < 0.01f)
            return;

        selectedIndex -= (int)Input.mouseScrollDelta.y;
        WrapIndex();
        OnChangeInventoryItem?.Invoke();
    }

    void WrapIndex()
    {
        if (buildItems.Count == 0) { selectedIndex = 0; return; }
        if (selectedIndex < 0) selectedIndex = buildItems.Count - 1;
        if (selectedIndex >= buildItems.Count) selectedIndex = 0;
    }
}
