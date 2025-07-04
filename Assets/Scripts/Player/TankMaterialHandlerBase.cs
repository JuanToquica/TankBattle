using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class TankMaterialHandlerBase : MonoBehaviour
{
    [SerializeField] protected MaterialsData materialsData;
    [SerializeField] protected GameObject vfx;
    [SerializeField] protected MeshRenderer mainTurretMeshRenderer;
    [SerializeField] protected MeshRenderer railgunMeshRenderer;
    [SerializeField] protected MeshRenderer machineGunMeshRenderer;
    [SerializeField] protected MeshRenderer rocketMeshRenderer;
    [SerializeField] protected MeshRenderer mainCannonMeshRenderer;
    [SerializeField] protected MeshRenderer railgunCannonMeshRenderer;
    [SerializeField] protected MeshRenderer machineGunCannonMeshRenderer;
    [SerializeField] protected MeshRenderer rocketCannon1MeshRenderer;
    [SerializeField] protected MeshRenderer rocketCannon2MeshRenderer;
    [SerializeField] protected MeshRenderer tankMeshRenderer;
    [SerializeField] protected MeshRenderer SmallWheelsRightMeshRenderer;
    [SerializeField] protected MeshRenderer WheelFrontRightMeshRenderer;
    [SerializeField] protected MeshRenderer WheelBackRightMeshRenderer;
    [SerializeField] protected MeshRenderer SmallWheelsLeftMeshRenderer;
    [SerializeField] protected MeshRenderer WheelFrontLeftMeshRenderer;
    [SerializeField] protected MeshRenderer WheelBackLeftMeshRenderer;
    [SerializeField] protected SkinnedMeshRenderer TrackRightMeshRenderer;
    [SerializeField] protected SkinnedMeshRenderer TrackLeftMeshRenderer;
    [SerializeField] protected float dissolveRate;
    [SerializeField] protected float refreshRate;
    List<Material> materials = new List<Material>();

    private void Start()
    {
        AddRendererMaterials(mainTurretMeshRenderer);
        AddRendererMaterials(railgunMeshRenderer);
        AddRendererMaterials(machineGunMeshRenderer);
        AddRendererMaterials(rocketMeshRenderer);
        AddRendererMaterials(mainCannonMeshRenderer);
        AddRendererMaterials(railgunCannonMeshRenderer);
        AddRendererMaterials(machineGunCannonMeshRenderer);
        AddRendererMaterials(rocketCannon1MeshRenderer);
        AddRendererMaterials(rocketCannon2MeshRenderer);
        AddRendererMaterials(tankMeshRenderer);
        AddRendererMaterials(SmallWheelsRightMeshRenderer);
        AddRendererMaterials(WheelFrontRightMeshRenderer);
        AddRendererMaterials(WheelBackRightMeshRenderer);
        AddRendererMaterials(SmallWheelsLeftMeshRenderer);
        AddRendererMaterials(WheelFrontLeftMeshRenderer);
        AddRendererMaterials(WheelBackLeftMeshRenderer);
    }

    private void OnEnable()
    {
        vfx.SetActive(false);
        for (int i = 0; i < materials.Count; i++)
        {
            materials[i].SetFloat("_DissolveAmount", 0);
        }
        TrackRightMeshRenderer.materials[0].SetFloat("_DissolveAmount", 0);
        TrackLeftMeshRenderer.materials[0].SetFloat("_DissolveAmount", 0);
    }

    private void AddRendererMaterials(MeshRenderer renderer)
    {
        if (renderer != null)
        {
            materials.AddRange(renderer.materials);
        }
    }

    public void OnTankDead()
    {
        StartCoroutine(DissolvePaint());
    }

    protected IEnumerator DissolvePaint()
    {
        vfx.SetActive(true);
        VisualEffect particles = vfx.GetComponent<VisualEffect>();
        particles.Stop();
        particles.Play();

        float counter = 0;

        while (materials[0].GetFloat("_DissolveAmount") < 1)
        {
            if (TrackRightMeshRenderer.materials.Length > 1)
                TrackRightMeshRenderer.materials = new Material[] { TrackRightMeshRenderer.materials[0] };
            if (TrackLeftMeshRenderer.materials.Length > 1)
                TrackLeftMeshRenderer.materials = new Material[] { TrackLeftMeshRenderer.materials[0] };
            counter += dissolveRate;
            for (int i = 0; i < materials.Count; i++)
            {
                materials[i].SetFloat("_DissolveAmount", counter);
            }
            TrackRightMeshRenderer.materials[0].SetFloat("_DissolveAmount", counter);
            TrackLeftMeshRenderer.materials[0].SetFloat("_DissolveAmount", counter);
            yield return new WaitForSeconds(refreshRate);
        }
    }

    public virtual void LoadTankMaterial(int index)
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
