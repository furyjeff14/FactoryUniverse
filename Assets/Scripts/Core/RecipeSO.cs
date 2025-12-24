using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Factory/Recipe")]
public class RecipeSO : ScriptableObject
{
    public float craftTime;
    public List<ResourceAmount> inputs = new();
    public List<ResourceAmount> outputs = new();
}
