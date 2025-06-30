using UnityEngine;

public class CubeFactory : MonoBehaviour
{
    public static CubeFactory Instance { get; private set; }
    [SerializeField] private GameObject _cubePrefab;
    [SerializeField] private int _initialSize = 20;
    private CubePool _cubePool;

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
        GameObject go = _cubePool.Get();              // Obje havuzdan (veya instantiate ile) alýnýyor
        go.transform.position = worldPosition;       // Pozisyon ayarlanýyor
        var controller = go.GetComponent<CubeController>();
        controller.Initialize(data);                  // Renk ve yön bilgisi yükleniyor
        return controller;
    }

    public void ReleaseCube(CubeController controller)
    {
        _cubePool.Release(controller.gameObject);    // Havuz’a geri koyma da burada
    }
}