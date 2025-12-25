using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HotbarSlotUI : MonoBehaviour
{
    public Image background;
    public Image icon;
    public TMP_Text label;

    public Color normalColor = Color.gray;
    public Color selectedColor = Color.white;

    public void SetItem(InventoryItemSO item)
    {
        if (item == null)
        {
            icon.enabled = false;
            label.text = "";
            return;
        }

        icon.enabled = true;
        icon.sprite = item.icon;
        label.text = item.itemName;
    }

    public void SetSelected(bool selected)
    {
        background.color = selected ? selectedColor : normalColor;
    }
}
