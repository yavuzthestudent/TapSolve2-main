using DG.Tweening;
using UnityEngine;
using static UnityEngine.ParticleSystem;
using static UnityEngine.UI.Image;

public class CubeController : MonoBehaviour, IClickable
{
    [SerializeField] private MeshRenderer _meshRenderer;
    [SerializeField] private TrailRenderer _trail;
    [SerializeField] private Transform _arrowTransform;

    [SerializeField] private float _moveDistance = 1.5f;    // Hücre boyutu 1.5 olarak hardcode
    [SerializeField] private float _moveSpeed = 1f;
    [SerializeField] private float _flashDuration = 0.15f;

    [SerializeField] private LayerMask _obstacleLayer;

    private CubeData _cubeData;
    private Tween _moveTween;
    private Sequence _flashSequence;
    private Color _originalColor;

    private bool _isMoving;

    private Vector3 _dir;
    private Vector2Int _currentGridPosition;
    public Vector2Int _cubeLastPosition; // Çarpışma anında son pozisyonu kaydetmek için

    // Initialize sırasında level referansını alacağız
    private Level _levelReference;

    // Sabit mesafe değerleri - Cube size 1.5 için
    private const float CUBE_SIZE = 1.5f;           // Küp boyutu
    private const float COLLISION_THRESHOLD = 1.6f; // Çarpışma eşiği - küp boyutundan biraz büyük
    private const float SAFE_MARGIN = 0.05f;        // Küçük güvenli marj

    // Hareket kontrolü için
    private Vector3 _startPosition;
    private Vector3 _targetPosition;
    private bool _collisionDetected = false;

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

        // Hedef pozisyonu hesapla
        _startPosition = transform.position;
        _targetPosition = _startPosition + _dir * _moveDistance;
        _collisionDetected = false;

        // Hareketi başlat
        MoveToPosition();
    }

    private void MoveToPosition()
    {
        _isMoving = true;

        _moveTween = transform
            .DOMove(_targetPosition, _moveSpeed)
            .SetEase(Ease.InOutQuad)
            .OnUpdate(() =>
            {
                // Çarpışma henüz tespit edilmemişse kontrol et
                if (!_collisionDetected)
                {
                    CheckCollisionDuringMovement();
                }
            })
            .OnComplete(() =>
            {
                _isMoving = false;

                // Eğer çarpışma olmadıysa küpü kaldır
                if (!_collisionDetected)
                {
                    EventManager.RaiseCubeCleared(this);
                    CubeFactory.Instance.ReleaseCube(this);
                }
            });
    }

    private void CheckCollisionDuringMovement()
    {
        // Mevcut pozisyondan hedef pozisyona olan mesafeyi hesapla
        float remainingDistance = Vector3.Distance(transform.position, _targetPosition);

        // Raycast ile önümüzdeki engeli kontrol et
        RaycastHit hit;
        if (Physics.Raycast(transform.position, _dir, out hit, remainingDistance, _obstacleLayer))
        {
            // Kendi collider'ını ignore et
            if (hit.collider.gameObject == this.gameObject)
                return;

            var otherController = hit.collider.GetComponent<CubeController>();

            // Çarpışma mesafesi kontrolü
            if (hit.distance <= COLLISION_THRESHOLD)
            {
                _collisionDetected = true;

                // Hareketi durdur
                _moveTween.Kill();

                // Güvenli pozisyonu hesapla
                Vector3 safePosition = CalculateSafePosition(hit);
                transform.position = safePosition;

                // Flash efekti
                if (otherController != null)
                    FlashRed(otherController);

                // Pozisyonu kaydet
                SaveCurrentPosition(safePosition);

                _isMoving = false;

                Debug.Log($"Collision detected! Hit distance: {hit.distance:F3}, Safe position: {safePosition}");
            }
        }
    }

    private Vector3 CalculateSafePosition(RaycastHit hit)
    {
        // Çarpışan küpün hemen yanında dur - geri gitme!
        Vector3 hitCubePosition = hit.collider.transform.position;
        Vector3 safePosition = hitCubePosition - _dir * (CUBE_SIZE + SAFE_MARGIN);

        // Grid'e hizala (opsiyonel - daha düzenli görünüm için)
        return SnapToGrid(safePosition);
    }

    private Vector3 SnapToGrid(Vector3 position)
    {
        // Grid hizalaması için küp boyutunun yarısına yuvarla
        float snapValue = CUBE_SIZE / 2f; // 0.75f

        float snappedX = Mathf.Round(position.x / snapValue) * snapValue;
        float snappedY = Mathf.Round(position.y / snapValue) * snapValue;
        float snappedZ = position.z; // Z ekseni değişmez

        return new Vector3(snappedX, snappedY, snappedZ);
    }

    private void SaveCurrentPosition(Vector3 worldPosition)
    {
        Vector2Int gridPos = CalculateGridPosition(worldPosition);
        _cubeData.LastPosition = gridPos;
        _cubeLastPosition = gridPos;
        _levelReference.SaveGameState();
        Debug.Log($"Position saved: Grid({gridPos.x}, {gridPos.y}) World({worldPosition.x:F2}, {worldPosition.y:F2})");
    }

    private Vector2Int CalculateGridPosition(Vector3 worldPos)
    {
        float cell = 1.5f; // Hücre boyutu hardcode
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
        _collisionDetected = false;
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