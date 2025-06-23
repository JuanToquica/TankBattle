using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class ChangeTankPaint : MonoBehaviour
{
    [SerializeField] private MaterialsData materialsData;
    [SerializeField] private GameObject vfx;
    [SerializeField] private MeshRenderer turretMeshRenderer;
    [SerializeField] private MeshRenderer cannonMeshRenderer;
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

    private void Start()
    {
        AddRendererMaterials(turretMeshRenderer);
        AddRendererMaterials(cannonMeshRenderer);
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
        if (transform.CompareTag("Player"))
            LoadTankMaterial(DataManager.Instance.GetCurrentColorSelected());
    }

    public void LoadTankMaterial(int index)
    {
        Material material1 = materialsData.tankPaintings[index].materials[0];
        Material material2 = materialsData.tankPaintings[index].materials[1];

        tankMeshRenderer.materials = materialsData.tankPaintings[index].materials;
        turretMeshRenderer.materials = new Material[] { material2, material1 };
        cannonMeshRenderer.materials = new Material[] { material2 };
        SmallWheelsRightMeshRenderer.materials = new Material[] { material1 };
        WheelFrontRightMeshRenderer.materials = new Material[] { material1 };
        WheelBackRightMeshRenderer.materials = new Material[] { material1 };
        SmallWheelsLeftMeshRenderer.materials = new Material[] { material1 };
        WheelFrontLeftMeshRenderer.materials = new Material[] { material1 };
        WheelBackLeftMeshRenderer.materials = new Material[] { material1 };
        TrackRightMeshRenderer.materials = new Material[] { material1 };
        TrackLeftMeshRenderer.materials = new Material[] { material1 };
    }

    public void OnTankDead()
    {
        StartCoroutine(DissolvePaint());
    }

    private IEnumerator DissolvePaint()
    {
        if (TrackRightMeshRenderer.materials.Length > 1)
            TrackRightMeshRenderer.materials = new Material[] { TrackRightMeshRenderer.materials[0] };
        if (TrackLeftMeshRenderer.materials.Length > 1)
            TrackLeftMeshRenderer.materials = new Material[] { TrackLeftMeshRenderer.materials[0] };
        vfx.SetActive(true);
        VisualEffect particles = vfx.GetComponent<VisualEffect>();
        particles.Play();

        float counter = 0;

        while (materials[0].GetFloat("_DissolveAmount") < 1)
        {
            counter += dissolveRate;
            for (int i = 0; i < materials.Count; i++)
            {
                materials[i].SetFloat("_DissolveAmount", counter);
            }
            TrackRightMeshRenderer.materials[0].SetFloat("_DissolveAmount", counter);
            TrackLeftMeshRenderer.materials[0].SetFloat("_DissolveAmount", counter);
            yield return new WaitForSeconds(refreshRate);
        }
        gameObject.SetActive(false);
    }
}
