using UnityEngine;

public class CubeFactory : MonoBehaviour
{
    public static CubeFactory Instance { get; private set; }

    [SerializeField] private GameObject _cubePrefab;
    [SerializeField] private int _initialSize = 30;

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

        go.transform.position = worldPosition;       // Pozisyon ayarlanýyor

        controller.ResetState();
        controller.Initialize(data);                  // Renk ve yön bilgisi yükleniyor

        go.SetActive(true);

        //foreach(var control in controller)
        //{
        //    go.SetActive(true);
        //}

        return controller;
    }

    public void ReleaseCube(CubeController controller)
    {
        if (controller != null && controller.gameObject != null)
        {
            _cubePool.Release(controller.gameObject);    // Havuz’a geri koyma da burada
        }
    }
}