//// Assets/Scripts/Pooling/ObjectPooler.cs
//using System;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.Pool;

//public class ObjectPooler : MonoBehaviour
//{
//    public static ObjectPooler Instance { get; private set; }

//    [SerializeField] private GameObject _cubePrefab;
//    [SerializeField] private int _initialPoolSize = 20;

//    private ObjectPool<GameObject> _pool;

//    private void Awake()
//    {
//        if (Instance != null && Instance != this)
//        {
//            Destroy(gameObject);
//            return;
//        }
//        Instance = this;

//        _pool = new ObjectPool<GameObject>(
//            CreateCube,
//            OnGetCube,
//            OnReleaseCube,
//            OnDestroyCube,
//            false,
//            _initialPoolSize
//        );
//    }

//    private GameObject CreateCube()
//    {
//        var go = Instantiate(_cubePrefab);
//        go.SetActive(false);
//        return go;
//    }
//    private void OnGetCube(GameObject cube)
//    {
//        cube.SetActive(true);
//    }
//    private void OnReleaseCube(GameObject cube)
//    {
//        cube.SetActive(false);
//    }
//    private void OnDestroyCube(GameObject cube)
//    {
//        Destroy(cube);
//    }

//    public CubeController SpawnCube(CubeData cubeData)
//    {
//        var go = _pool.Get();

//        Vector3 worldPos = new Vector3(cubeData.GridPosition.x, 0, cubeData.GridPosition.y);
//        go.transform.position = worldPos;

//        var controller = go.GetComponent<CubeController>();
//        controller.Initialize(cubeData);
//        return controller;
//    }

//    public void ReleaseCube(CubeController controller)
//    {
//        _pool.Release(controller.gameObject);
//    }
//}
