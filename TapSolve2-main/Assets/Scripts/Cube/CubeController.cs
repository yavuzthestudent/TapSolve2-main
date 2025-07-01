using DG.Tweening;
using UnityEngine;
using static UnityEngine.UI.Image;

public class CubeController : MonoBehaviour, IClickable
{
    [SerializeField] private MeshRenderer _meshRenderer;
    [SerializeField] private Transform _arrowTransform;
    [SerializeField] private float _moveDistance = 1f;
    [SerializeField] private float _moveSpeed = 1f;
    [SerializeField] private float _flashDuration = 0.15f;
    [SerializeField] private LayerMask _obstacleLayer;

    private CubeData _cubeData;
    private Tween _moveTween;

    private bool _isMoving;
    private bool _canMove;

    private Vector3 origin;
    private Vector3 dir;

    public void Initialize(CubeData data)
    {
        _cubeData = data;
        _meshRenderer.material.color = data.Color;
        _arrowTransform.localRotation = GetRotationForDirection(data.Direction);
    }

    private void Start()
    {
        origin = transform.position;
        dir = DirectionToVector(_cubeData.Direction);
    }

    public void OnClick()
    {
        if (_isMoving)
            return;

        // Hamleyi kullan
        GameManager.Instance.UseMove();

        // Hedef pozisyonu hesapla
        Vector3 targetPos = transform.position + dir * _moveDistance;

        // Hareketi ayrı metotta başlat
        MoveToPosition(targetPos);
    }

    private void MoveToPosition(Vector3 target)
    {
        _isMoving = true;

        _moveTween = transform
            .DOMove(target, _moveSpeed)
            .SetEase(Ease.InOutQuad)
            .OnUpdate(() =>
            {
                _canMove = MoveCheck(); // Her güncellemede engel kontrolü yap
                if (!_canMove)
                {
                    // Eğer engel varsa hareketi durdur
                    _isMoving = false;
                    _moveTween.Kill(); // Tween'i iptal et
                    return;
                }
            })
            .OnComplete(() =>
            {
                // Hareketi kesinleştir
                transform.position = target;
                _isMoving = false;

                // Küp temizlendi olayı
                EventManager.RaiseCubeCleared(this);

                // Havuz/Gerikoyma
                CubeFactory.Instance.ReleaseCube(this);
            });
    }

    private bool MoveCheck()
    {
        RaycastHit hit;
        // Her frame'de önünde engel var mı bak
        if (Physics.Raycast(transform.position, dir, out hit, _moveDistance, _obstacleLayer))
        {
            // Eğer aradaki mesafe 1 birim veya daha azsa dur
            if (hit.distance <= 0.51f && hit.collider.gameObject != this.gameObject)
            {
                FlashRed(hit.collider.GetComponent<MeshRenderer>());

                Debug.Log(hit.distance);
                // Hedef pozisyonu engelin hemen önüne ayarla
                Vector3 stopPos = hit.point - dir.normalized;
                _moveTween.Kill();
                _isMoving = false;
                return false;
            }
        }
        return true;
    }

    private void FlashRed(MeshRenderer otherCubeRenderer)
    {
        Color otherCubesColor = otherCubeRenderer.material.color;
        Color thisOriginalColor = _meshRenderer.material.color; // Kendi rengimizi de kaydedelim

        Sequence sequence = DOTween.Sequence();

        //Çarpışan küpleri kırmızıya flash yap
        sequence.Append(_meshRenderer.material
            .DOColor(Color.red, _flashDuration)
            .SetEase(Ease.InOutQuad));
        sequence.Join(otherCubeRenderer.material // Join() ile aynı anda başlat
            .DOColor(Color.red, _flashDuration)
            .SetEase(Ease.InOutQuad));

        // Ardından orijinal renge geri dön
        sequence.Append(_meshRenderer.material
            .DOColor(thisOriginalColor, _flashDuration)
            .SetEase(Ease.InOutQuad));
        sequence.Join(otherCubeRenderer.material
            .DOColor(otherCubesColor, _flashDuration)
            .SetEase(Ease.InOutQuad));
    }

    private Quaternion GetRotationForDirection(Direction direction)
    {
        switch (direction)
        {
            case Direction.Up: return Quaternion.Euler(0, 0, 0);
            case Direction.Down: return Quaternion.Euler(0, 180, 0);
            case Direction.Left: return Quaternion.Euler(0, -90, 0);
            case Direction.Right: return Quaternion.Euler(0, 90, 0);
            default: return Quaternion.identity;
        }
    }
    private Vector3 DirectionToVector(Direction direction)
    {
        switch (direction)
        {
            case Direction.Up: return Vector3.down;
            case Direction.Down: return Vector3.up;
            case Direction.Left: return Vector3.right;
            case Direction.Right: return Vector3.left;
            default: return Vector3.zero;
        }
    }
}
