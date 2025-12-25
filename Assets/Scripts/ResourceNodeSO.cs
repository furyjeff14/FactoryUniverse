using UnityEngine;

[CreateAssetMenu(menuName = "Factory/Resource Node")]
public class ResourceNodeSO : GraphNodeSO
{
    [SerializeField] private string resourceName;
    public string ResourceName => resourceName;
    public void SetResourceName(string value) => resourceName = value;

    public ResourceNodeSO()
    {
        resourceName = "New Resource";
    }

    public ResourceNodeSO(string _resourceName)
    {
        resourceName = _resourceName;
    }
}
