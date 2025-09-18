using UnityEngine;


public class PlayerMaterialHandler : TankMaterialHandlerBase
{
    private void Awake()
    {
        LoadTankMaterial(DataManager.Instance.GetCurrentColorSelected());
    }

    
    public override void LoadTankMaterial(int index)
    {
        base.LoadTankMaterial(index);
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
    }
}
