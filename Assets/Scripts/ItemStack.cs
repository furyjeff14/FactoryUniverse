[System.Serializable]
public class ItemStack
{
    public string itemId;
    public int amount;

    public ItemStack(string id, int amt = 1)
    {
        itemId = id;
        amount = amt;
    }
}
