using UnityEngine;

public class PlayerResourceCollector : MonoBehaviour
{
    public Transform playerCamera;
    public float interactDistance = 3f;
    public float collectRate = 5f;

    private PlayerInventory inventory;

    void Start()
    {
        inventory = GetComponent<PlayerInventory>();
        if (inventory == null) inventory = gameObject.AddComponent<PlayerInventory>();
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.E))
        {
            Ray ray = new Ray(playerCamera.position, playerCamera.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
            {
                ResourceNode node = hit.collider.GetComponent<ResourceNode>();
                if (node != null)
                {
                    float collected = node.CollectResource(collectRate * Time.deltaTime);
                    inventory.AddResource(node.resourceData.ResourceName, collected);
                }
            }
        }
    }
}
