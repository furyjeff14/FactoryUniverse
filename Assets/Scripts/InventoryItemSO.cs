using UnityEngine;

[CreateAssetMenu(menuName = "Factory/Inventory Item")]
public class InventoryItemSO : ScriptableObject
{
    public string itemName;
    public GameObject prefab;
    public Sprite icon;
}
