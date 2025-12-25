using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    /* =======================
     * RESOURCES
     * ======================= */

    private Dictionary<string, float> resources = new();
    public Action OnChangeInventoryItem;

    public void AddResource(string resourceName, float amount)
    {
        if (!resources.ContainsKey(resourceName))
            resources[resourceName] = 0f;

        resources[resourceName] += amount;
    }

    public float GetResourceAmount(string resourceName)
    {
        return resources.TryGetValue(resourceName, out float v) ? v : 0f;
    }

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
     * BUILD INVENTORY (HOTBAR)
     * ======================= */

    [Header("Build Inventory (Hotbar)")]
    public List<BuildItemSO> buildItems = new List<BuildItemSO>();
    public int selectedIndex = -1;

    public BuildItemSO SelectedItem =>
        buildItems.Count > 0 ? buildItems[selectedIndex] : null;

    void Update()
    {
        HandleNumberKeys();
    }

    void HandleNumberKeys()
    {
        int max = Mathf.Min(buildItems.Count, 9);

        for (int i = 0; i < max; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                if (selectedIndex == -1 || selectedIndex != i)
                {
                    GetComponent<MachinePlacement>().SetEnablePlacement(true);
                } else
                {
                    GetComponent<MachinePlacement>().SetEnablePlacement();
                }

                if(selectedIndex != i)
                {
                    OnChangeInventoryItem?.Invoke();
                }

                selectedIndex = i;
                ClampIndex();
            }
        }
    }

    void ClampIndex()
    {
        if (buildItems.Count == 0)
            selectedIndex = 0;
        else
            selectedIndex = Mathf.Clamp(selectedIndex, 0, buildItems.Count - 1);
    }
}
