using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Instance { get; private set; }
    [SerializeField] private Transform poolParent;

    private Dictionary<GameObject, Queue<GameObject>> poolDictionary = new Dictionary<GameObject, Queue<GameObject>>();
    private Dictionary<GameObject, GameObject> originalPrefabs = new Dictionary<GameObject, GameObject>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void CreatePool(GameObject prefab, int initialSize)
    {
        if (prefab == null) { return; }

        if (poolDictionary.ContainsKey(prefab))
        {
            Debug.LogWarning($"Pool para '{prefab.name}' ya existe.");
            return;
        }

        Queue<GameObject> newPool = new Queue<GameObject>();
        GameObject prefabPoolParent = new GameObject(prefab.name + " Pool");
        prefabPoolParent.transform.parent = poolParent;

        for (int i = 0; i < initialSize; i++)
        {
            GameObject obj = Instantiate(prefab);
            obj.SetActive(false);
            obj.transform.SetParent(prefabPoolParent.transform);
            newPool.Enqueue(obj);
            originalPrefabs.Add(obj, prefab);
        }
        poolDictionary.Add(prefab, newPool);
        Debug.Log($"Pool '{prefab.name}' creado con {initialSize} objetos.");
    }

    public GameObject GetPooledObject(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(prefab))
        {
            Debug.Log($"Pool para '{prefab.name}' no existe. Creando uno nuevo con tamaño mínimo.");
            CreatePool(prefab, 5);
        }

        Queue<GameObject> pool = poolDictionary[prefab];
        GameObject objToSpawn = null;

        if (pool.Count > 0)
        {
            objToSpawn = pool.Dequeue();
            objToSpawn.SetActive(true);
            objToSpawn.transform.position = position;
            objToSpawn.transform.rotation = rotation;
        }
        else
        {
            Debug.Log($"Pool para '{prefab.name}' vacío. Instanciando uno nuevo.");
            objToSpawn = Instantiate(prefab, position, rotation);
            originalPrefabs.Add(objToSpawn, prefab);
        }
        return objToSpawn;
    }


    public void ReturnPooledObject(GameObject obj)
    {
        if (obj == null) return;

        if (originalPrefabs.TryGetValue(obj, out GameObject prefab))
        {
            if (poolDictionary.TryGetValue(prefab, out Queue<GameObject> pool))
            {
                obj.SetActive(false);
                obj.transform.SetParent(poolParent.Find(prefab.name + " Pool"));
                if (obj.transform.parent == null)
                {
                    obj.transform.parent = poolParent;
                }
                pool.Enqueue(obj);
            }
            else
            {
                // El pool para este prefab fue destruido o no existe, destruirlo
                Debug.Log($"No se encontró un pool para el prefab de '{obj.name}'. Destruyendo el objeto.");
                Destroy(obj);
            }
        }
        else
        {
            Debug.Log($"Objeto '{obj.name}' no gestionado por este pool o ya retornado. Destruyendo.");
            Destroy(obj);
        }
    }

}
