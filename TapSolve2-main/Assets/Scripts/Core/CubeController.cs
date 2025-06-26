using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;
public class CubeController : MonoBehaviour, IClickable
{
    [SerializeField] private MeshRenderer _meshRenderer;
    [SerializeField] private Transform _arrowTransform;
    [SerializeField] private float _moveDistance = 1f;
    [SerializeField] private float _moveSpeed = 5f;

    [SerializeField] private CubeData _cubeData;
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
        
        Vector3 dir = DirectionToVector(_cubeData.Direction);
        Vector3 targetPosition = transform.position + dir * _moveDistance;

        MoveToPosition(targetPosition);
        GameManager.Instance.UseMove();
    }

    private Vector3 DirectionToVector(Direction direction)
    {
        switch (direction)
        {
            case Direction.Up:
                return Vector3.back;
            case Direction.Down:
                return Vector3.forward;
            case Direction.Left:
                return Vector3.right;
            case Direction.Right:
                return Vector3.left;
            default:
                return Vector3.zero;
        }
    }

    private void MoveToPosition(Vector3 target)
    {
        transform.DOMove(target, _moveSpeed)
            .SetEase(Ease.Linear)
            .OnStart(() => _isMoving = true)
            .OnComplete(() =>
            {
                transform.position = target;
                _isMoving = false;
                EventManager.RaiseCubeCleared(this);
            });

        transform.position = target;
        _isMoving = false;
        
        EventManager.RaiseCubeCleared(this);
    }
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
            default:
                return Quaternion.identity;
        }
    }

}
