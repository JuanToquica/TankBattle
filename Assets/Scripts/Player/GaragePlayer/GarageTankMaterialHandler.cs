using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class GarageTankMaterialHandler : MonoBehaviour
{
    [SerializeField] private MaterialsData materialsData;
    [SerializeField] private MeshRenderer mainTurretMeshRenderer;
    [SerializeField] private MeshRenderer mainCannonMeshRenderer;
    [SerializeField] private MeshRenderer tankMeshRenderer;
    [SerializeField] private MeshRenderer SmallWheelsRightMeshRenderer;
    [SerializeField] private MeshRenderer WheelFrontRightMeshRenderer;
    [SerializeField] private MeshRenderer WheelBackRightMeshRenderer;
    [SerializeField] private MeshRenderer SmallWheelsLeftMeshRenderer;
    [SerializeField] private MeshRenderer WheelFrontLeftMeshRenderer;
    [SerializeField] private MeshRenderer WheelBackLeftMeshRenderer;
    [SerializeField] private SkinnedMeshRenderer TrackRightMeshRenderer;
    [SerializeField] private SkinnedMeshRenderer TrackLeftMeshRenderer;

    private void Start()
    {
        LoadTankMaterial(DataManager.Instance.GetCurrentColorSelected());
    }


    public void LoadTankMaterial(int index)
    {
        Material material1 = materialsData.tankPaintings[index].materials[0];
        Material material2 = materialsData.tankPaintings[index].materials[1];

        tankMeshRenderer.materials = materialsData.tankPaintings[index].materials;
        mainTurretMeshRenderer.materials = new Material[] { material2, material1 };
        mainCannonMeshRenderer.materials = new Material[] { material2 };
        SmallWheelsRightMeshRenderer.materials = new Material[] { material1 };
        WheelFrontRightMeshRenderer.materials = new Material[] { material1 };
        WheelBackRightMeshRenderer.materials = new Material[] { material1 };
        SmallWheelsLeftMeshRenderer.materials = new Material[] { material1 };
        WheelFrontLeftMeshRenderer.materials = new Material[] { material1 };
        WheelBackLeftMeshRenderer.materials = new Material[] { material1 };
        TrackRightMeshRenderer.materials = new Material[] { material1 };
        TrackLeftMeshRenderer.materials = new Material[] { material1 };
    }
}
