using UnityEngine;
using UnityEngine.Pool;

public class CubePool
{
    private ObjectPool<GameObject> _pool;

    public CubePool(GameObject prefab, int initialSize)
    {
        _pool = new ObjectPool<GameObject>(
            () => Object.Instantiate(prefab),
            cube => cube.SetActive(true),
            cube => cube.SetActive(false),
            cube => Object.Destroy(cube),
            false,
            initialSize,
            100
        );
    }

    public GameObject Get() => _pool.Get();
    public void Release(GameObject cube) => _pool.Release(cube);
}
