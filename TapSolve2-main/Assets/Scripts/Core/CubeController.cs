using System;
using System.Collections;
using UnityEngine;

public class CubeController : MonoBehaviour, IClickable
{
    [SerializeField] private MeshRenderer _meshRenderer;
    [SerializeField] private Transform _arrowTransform;
    [SerializeField] private float _moveDistance = 1f;
    [SerializeField] private float _moveSpeed = 5f;

    private CubeData _cubeData;
    private bool _isMoving;

    public void Initialize(CubeData cubeData)
    {
        _cubeData = cubeData;
        _meshRenderer.material.color = _cubeData.Color;
        _arrowTransform.localRotation = GetRotationForDirection(_cubeData.Direction);
    }

    public void OnClick()
    {
        if (_isMoving)
        {
            return;
        }
        GameManager.Instance.UseMove();
        
        Vector3 dir = direc
    }

    private IEnumerator MoveToPosition(Vector3 target)
    {
        _isMoving = true;
        while (Vector3.Distance(transform.position, target) > 0.01f)
        {
            transform.position = Vector3.Lerp(startPosition, target, elapsedTime / _moveSpeed);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.position = target;
        _isMoving = false;
    private Quaternion GetRotationForDirection(Direction direction)
    {
        switch (direction)
        {
            case Direction.Up:
                return Quaternion.Euler(0, 0, 0);

            case Direction.Down:
                return Quaternion.Euler(0, 180, 0);

            case Direction.Left:
                return Quaternion.Euler(0, -90, 0);

            case Direction.Right:
                return Quaternion.Euler(0, 90, 0);
        }
    }

}
