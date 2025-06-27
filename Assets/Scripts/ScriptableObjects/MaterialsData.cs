using JetBrains.Annotations;
using UnityEngine;

[System.Serializable]
public class Materials
{
    public Material[] materials;
    public Material railgunTurretMaterial;
    public Material machineGunTurretMaterial;
    public Material rocketTurretMaterial;
}


[CreateAssetMenu(fileName = "MaterialsData", menuName = "MaterialsData")]
public class MaterialsData : ScriptableObject
{
    public Materials[] tankPaintings;
    public int cost;
}
