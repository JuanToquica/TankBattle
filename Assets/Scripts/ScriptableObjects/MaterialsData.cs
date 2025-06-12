using JetBrains.Annotations;
using UnityEngine;

[System.Serializable]
public class Materials
{
    public Material[] materials;
}


[CreateAssetMenu(fileName = "MaterialsData", menuName = "MaterialsData")]
public class MaterialsData : ScriptableObject
{
    public Materials[] tankPaintings;
    public int cost;
}
