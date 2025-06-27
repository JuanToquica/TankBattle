using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

public class ChangeTankPaint : MonoBehaviour
{
    [SerializeField] private MaterialsData materialsData;
    [SerializeField] private GameObject vfx;
    [SerializeField] private MeshRenderer mainTurretMeshRenderer;
    [SerializeField] private MeshRenderer railgunMeshRenderer;
    [SerializeField] private MeshRenderer machineGunMeshRenderer;
    [SerializeField] private MeshRenderer rocketMeshRenderer;
    [SerializeField] private MeshRenderer mainCannonMeshRenderer;
    [SerializeField] private MeshRenderer railgunCannonMeshRenderer;
    [SerializeField] private MeshRenderer machineGunCannonMeshRenderer;
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
    [SerializeField] private float dissolveRate;
    [SerializeField] private float refreshRate;
    List<Material> materials = new List<Material>();

    private void Awake()
    {
        if (transform.CompareTag("Player"))
            LoadTankMaterial(DataManager.Instance.GetCurrentColorSelected());
    }

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

    private void AddRendererMaterials(MeshRenderer renderer)
    {
        if (renderer != null)
        {
            materials.AddRange(renderer.materials);
        }
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

    public void LoadTankMaterial(int index)
    {
        Material material1 = materialsData.tankPaintings[index].materials[0];
        Material material2 = materialsData.tankPaintings[index].materials[1];
        Material railgunMaterial = materialsData.tankPaintings[index].railgunTurretMaterial;
        Material machineGunMaterial = materialsData.tankPaintings[index].machineGunTurretMaterial;
        Material rocketMaterial = materialsData.tankPaintings[index].rocketTurretMaterial;


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

        railgunMeshRenderer.materials = new Material[] { railgunMaterial };
        machineGunMeshRenderer.materials = new Material[] { machineGunMaterial };
        rocketMeshRenderer.materials = new Material[] { rocketMaterial };
    }

    public void OnTankDead()
    {
        StartCoroutine(DissolvePaint());
    }

    private IEnumerator DissolvePaint()
    {  
        vfx.SetActive(true);
        VisualEffect particles = vfx.GetComponent<VisualEffect>();
        particles.Stop();
        particles.Play();

        float counter = 0;

        while (materials[0].GetFloat("_DissolveAmount") < 1)
        {
            Debug.Log("disolviendo");
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
}
