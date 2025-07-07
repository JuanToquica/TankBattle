using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class GarageTankMaterialHandler : MonoBehaviour
{
    [SerializeField] private MaterialsData materialsData;
    [SerializeField] private MeshRenderer mainTurretMeshRenderer;
    [SerializeField] private MeshRenderer mainCannonMeshRenderer;
    [SerializeField] private MeshRenderer railgunMeshRenderer;
    [SerializeField] private MeshRenderer railgunCannonMeshRenderer;
    [SerializeField] private MeshRenderer machineGunMeshRenderer;
    [SerializeField] private MeshRenderer machineGunCannonMeshRenderer;
    [SerializeField] private MeshRenderer rocketMeshRenderer;
    [SerializeField] private MeshRenderer rocketCannon1MeshRenderer;
    [SerializeField] private MeshRenderer rocketCannon2MeshRenderer;
    [SerializeField] private MeshRenderer tankMeshRenderer;
    [SerializeField] private MeshRenderer SmallWheelsRightMeshRenderer;
    [SerializeField] private MeshRenderer WheelFrontRightMeshRenderer;
    [SerializeField] private MeshRenderer WheelBackRightMeshRenderer;
    [SerializeField] private MeshRenderer SmallWheelsLeftMeshRenderer;
    [SerializeField] private MeshRenderer WheelFrontLeftMeshRenderer;
    [SerializeField] private MeshRenderer WheelBackLeftMeshRenderer;
    [SerializeField] private SkinnedMeshRenderer TrackRightMeshRenderer;
    [SerializeField] private SkinnedMeshRenderer TrackLeftMeshRenderer;
    [SerializeField] private GameObject[] turrets;

    private void Start()
    {
        LoadTankMaterial(DataManager.Instance.GetCurrentColorSelected());
    }


    public void LoadTankMaterial(int index)
    {
        Material material1 = materialsData.tankPaintings[index].materials[0];
        Material material2 = materialsData.tankPaintings[index].materials[1];
        Material railgunMaterial = materialsData.tankPaintings[index].railgunTurretMaterial;
        Material machineGunMaterial = materialsData.tankPaintings[index].machineGunTurretMaterial;
        Material rocketMaterial = materialsData.tankPaintings[index].rocketTurretMaterial;

        railgunMeshRenderer.materials = new Material[] { railgunMaterial };
        railgunCannonMeshRenderer.materials = new Material[] { railgunMaterial };
        machineGunMeshRenderer.materials = new Material[] { machineGunMaterial };
        machineGunCannonMeshRenderer.materials = new Material[] { machineGunMaterial };
        rocketMeshRenderer.materials = new Material[] { rocketMaterial };
        rocketCannon1MeshRenderer.materials = new Material[] { rocketMaterial };
        rocketCannon2MeshRenderer.materials = new Material[] { rocketMaterial };

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

    public void ChangeTurret(int index)
    {
        for (int i = 0; i < turrets.Length; i++)
        {
            if (i == index)
            {
                turrets[i].SetActive(true);
                continue;
            }
            turrets[i].SetActive(false);
        }
    }
}
