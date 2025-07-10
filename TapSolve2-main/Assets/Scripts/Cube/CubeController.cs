using DG.Tweening;
using UnityEngine;
using static UnityEngine.ParticleSystem;
using static UnityEngine.UI.Image;

public class CubeController : MonoBehaviour, IClickable
{
    [SerializeField] private MeshRenderer _meshRenderer;
    [SerializeField] private TrailRenderer _trail;
    [SerializeField] private Transform _arrowTransform;

    [SerializeField] private float _moveDistance = 1f;
    [SerializeField] private float _moveSpeed = 1f;
    [SerializeField] private float _flashDuration = 0.15f;

    [SerializeField] private LayerMask _obstacleLayer;

    private CubeData _cubeData;
    private Tween _moveTween;
    private Sequence _flashSequence;
    private Color _originalColor;

    private bool _isMoving;
    private bool _canMove;

    private Vector3 _dir;
    private Vector2Int _currentGridPosition;
    public Vector2Int _cubeLastPosition; // Çarpışma anında son pozisyonu kaydetmek için

    // Initialize sırasında level referansını alacağız
    private Level _levelReference;

    public void Initialize(CubeData data, Level levelRef)
    {
        _levelReference = levelRef;
        _cubeLastPosition = data.LastPosition;
        _currentGridPosition = data.GridPosition;
        _cubeData = data;
        _meshRenderer.material.color = GetColorForDirection(_cubeData.Direction);
        _arrowTransform.localRotation = GetRotationForDirection(data.Direction);
        _dir = DirectionToVector(_cubeData.Direction);

        _originalColor = _meshRenderer.material.color;

        ConfigureTrailRenderer(_originalColor);
    }

    private void Start()
    {
        _originalColor = _meshRenderer.material.color; // Flash efekti için orijinal rengi sakla
        _trail = GetComponent<TrailRenderer>();
    }

    public void OnClick()
    {
        if (_isMoving)
            return;

        // Hamle sayısını azalt
        EventManager.RaiseMoveRequested();

        // Hedef pozisyonu yön vektörü ile hesapla
        Vector3 targetPos = transform.position + _dir * _moveDistance;

        // Hareketi başlat
        MoveToPosition(targetPos);
    }

    private void MoveToPosition(Vector3 target)
    {
        // Hareket etmeden önce çarpışma kontrolü yap
        if (!CheckMovement(transform.position, isPreCheck: true))
        {
            return; // Çarpışma tespit edildi, hareket etme
        }

        _isMoving = true;
        _moveTween = transform
            .DOMove(target, _moveSpeed)
            .SetEase(Ease.InOutQuad)
            .OnUpdate(() =>
            {
                // Hareket sırasında sürekli kontrol et
                if (!CheckMovement(transform.position, isPreCheck: false))
                {
                    _isMoving = false;
                    _moveTween.Kill();
                    return;
                }
            })
            .OnComplete(() =>
            {
                // Hareket tamamlandığında küpü kaldır
                transform.position = target;
                _isMoving = false;
                EventManager.RaiseCubeCleared(this);
                CubeFactory.Instance.ReleaseCube(this);
            });
    }

    private Vector2Int CalculateGridPosition(Vector3 worldPos)
    {
        float cell = 1f;
        int cols = _levelReference._currentLevelData.columns;
        int rows = _levelReference._currentLevelData.rows;

        // Grid'i ortalamak için offset hesapla
        float offsetX = -((cols - 1) * 0.5f * cell);
        float offsetY = -((rows - 1) * 0.5f * cell);

        // Dünya koordinatını grid pozisyonuna çevir
        int x = Mathf.RoundToInt((worldPos.x - offsetX) / cell);
        int y = Mathf.RoundToInt((worldPos.y - offsetY) / cell);
        return new Vector2Int(x, y);
    }

    private bool CheckMovement(Vector3 fromPosition, bool isPreCheck = false)
    {
        RaycastHit hit;
        if (Physics.Raycast(fromPosition, _dir, out hit, _moveDistance, _obstacleLayer))
        {
            var otherController = hit.collider.GetComponent<CubeController>();

            // Diğer küp hareket halindeyse çarpışma yok say (sadece hareket sırasında)
            if (!isPreCheck && otherController != null && otherController._isMoving)
            {
                return true;
            }

            // Çarpışma mesafesi kontrolü - çok yakınsa dur
            if (hit.distance <= 0.6f && hit.collider.gameObject != this.gameObject)
            {
                if (otherController != null)
                {
                    FlashRed(otherController);
                }

                // Sadece gerçek hareket sırasında pozisyon kaydet
                if (!isPreCheck)
                {
                    Vector2Int gridPos = CalculateGridPosition(transform.position);
                    _cubeData.LastPosition = gridPos;
                    _levelReference.SaveGameState();
                    Debug.Log($"Collision distance: {hit.distance}");
                }

                return false; // Hareket edilemez
            }
        }
        return true; // Hareket edilebilir
    }

    private void FlashRed(CubeController other)
    {
        // Önceki flash animasyonlarını temizle
        if (_flashSequence != null && _flashSequence.IsActive())
        {
            _flashSequence.Kill();
            _meshRenderer.material.color = _originalColor;
        }
        if (other._flashSequence != null && other._flashSequence.IsActive())
        {
            other._flashSequence.Kill();
            other._meshRenderer.material.color = other._originalColor;
        }

        // İki küpü de aynı anda kırmızıya çevir, sonra eski renklerine döndür
        _flashSequence = DOTween.Sequence()
                    .Append(_meshRenderer.material
                        .DOColor(Color.red, _flashDuration)
                        .SetEase(Ease.InOutQuad))
                    .Join(other._meshRenderer.material
                        .DOColor(Color.red, _flashDuration)
                        .SetEase(Ease.InOutQuad))

                    .Append(_meshRenderer.material
                        .DOColor(_originalColor, _flashDuration)
                        .SetEase(Ease.InOutQuad))
                    .Join(other._meshRenderer.material
                        .DOColor(other._originalColor, _flashDuration)
                        .SetEase(Ease.InOutQuad));
    }

    public void ResetState()
    {
        _isMoving = false;
        _moveTween?.Kill(); // Aktif tween varsa iptal et
    }

    private void ConfigureTrailRenderer(Color color)
    {
        if (_trail == null) return;

        // Basit bir Unlit material oluştur ve rengini ayarla
        var mat = new Material(Shader.Find("Unlit/Color"));
        mat.SetColor("_Color", color);
        _trail.material = mat;

        // İz ne kadar süre ekranda kalacak
        _trail.time = 0.45f;

        // Genişlik ayarları: kalın başlayıp incelsin
        _trail.startWidth = 0.46f;   // başlangıç kalınlığı
        _trail.endWidth = 0.0f;      // bitiş kalınlığı (sıfıra kadar incelsin)

        // Renk geçişi: başta canlı, sonda şeffaf
        var grad = new Gradient();
        grad.SetKeys(
            new[]
            {
                new GradientColorKey(color, 0f),
                new GradientColorKey(color * 0.6f, 1f)
            },
            new[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        _trail.colorGradient = grad;

        // Kameraya hizalı çizgi daha temiz görünür
        _trail.alignment = LineAlignment.View;

        // Trail'i temizle ve yeniden başlat
        _trail.Clear();
        _trail.emitting = false;
        _trail.emitting = true;
    }

    private Quaternion GetRotationForDirection(Direction direction)
    {
        switch (direction)
        {
            case Direction.Down: return Quaternion.Euler(0, 0, 0);
            case Direction.Up: return Quaternion.Euler(0, 180, 0);
            case Direction.Right: return Quaternion.Euler(0, -90, 0);
            case Direction.Left: return Quaternion.Euler(0, 90, 0);
            default: return Quaternion.identity;
        }
    }

    private Vector3 DirectionToVector(Direction direction)
    {
        switch (direction)
        {
            case Direction.Up: return Vector3.up;
            case Direction.Down: return Vector3.down;
            case Direction.Left: return Vector3.left;
            case Direction.Right: return Vector3.right;
            default: return Vector3.zero;
        }
    }

    private Color GetColorForDirection(Direction dir)
    {
        return dir switch
        {
            Direction.Up => Color.blue,
            Direction.Down => Color.green,
            Direction.Left => Color.black,
            Direction.Right => Color.grey,
            _ => Color.white,
        };
    }

    public CubeData GetCubeData()
    {
        return _cubeData;
    }
}