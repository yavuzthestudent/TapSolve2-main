using UnityEngine;
using UnityEngine.Pool;

public class CubePool
{
    private ObjectPool<GameObject> _pool;

    public CubePool(GameObject prefab, int initialSize)
    {
        _pool = new ObjectPool<GameObject>(
            createFunc: () => Object.Instantiate(prefab),
            actionOnGet: cube => cube.SetActive(true),
            actionOnRelease: cube => cube.SetActive(false),
            actionOnDestroy: cube => Object.Destroy(cube),
            collectionCheck: false,
            defaultCapacity: initialSize,
            maxSize: initialSize * 2
        );
    }

    public GameObject Get() => _pool.Get();
    public void Release(GameObject cube) => _pool.Release(cube);
}