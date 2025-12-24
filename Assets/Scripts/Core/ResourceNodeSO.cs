using UnityEngine;

[CreateAssetMenu(menuName = "Factory/Resource Node")]
public class ResourceNodeSO : GraphNodeSO
{
    [SerializeField] private string resourceName;
    public string ResourceName => resourceName;
    public void SetResourceName(string value) => resourceName = value;
}
