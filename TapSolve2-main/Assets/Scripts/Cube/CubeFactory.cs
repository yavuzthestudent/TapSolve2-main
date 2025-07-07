using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class CubeFactory : MonoBehaviour
{
    public static CubeFactory Instance { get; private set; }

    [SerializeField] private GameObject _cubePrefab;
    [SerializeField] private int _initialSize = 30;

    private CubePool _cubePool;
    private List<CubeController> _activeCubes = new List<CubeController>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _cubePool = new CubePool(_cubePrefab, _initialSize);
    }

    public CubeController SpawnCube(CubeData data, Vector3 worldPosition)
    {
        GameObject go = _cubePool.Get();// Obje havuzdan (veya instantiate ile) alýnýyor

        if(go == null)
        {
            Debug.LogError("Cube prefab is not assigned in CubeFactory.");
            return null;
        }

        var controller = go.GetComponent<CubeController>();

        if(controller == null)
        {
            Debug.LogError("CubeController component is missing on the cube prefab.");
            return null;
        }

        go.transform.position = worldPosition;

        controller.ResetState();
        controller.Initialize(data);

        go.SetActive(true);

        _activeCubes.Add(controller); // Aktif küpler listesine ekleniyor

        return controller;
    }

    public void ReleaseCube(CubeController controller)
    {
        if (controller != null && controller.gameObject != null)
        {
            _cubePool.Release(controller.gameObject); // Havuz’a geri koyma da burada

            _activeCubes.Remove(controller); // Aktif küpler listesinden çýkarýlýyor
        }
    }
    public void ClearAllCubes()
    {
        // Listeyi diziye kopyala, sonra ona göre temizle
        var copy = _activeCubes.ToArray();
        foreach (var cube in copy)
        {
            ReleaseCube(cube);
        }
    }
}