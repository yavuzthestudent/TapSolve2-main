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

    private Vector3 dir;

    public void Initialize(CubeData data)
    {
        _cubeData = data;
        _meshRenderer.material.color = data.Color;
        _arrowTransform.localRotation = GetRotationForDirection(data.Direction);
        dir = DirectionToVector(_cubeData.Direction);

        _originalColor = data.Color;

        ConfigureTrailRenderer(data.Color);
    }

    private void Start()
    {
        _originalColor = _meshRenderer.material.color; // Kendi rengimiz, flashRed() için lazım olacak
        _trail = GetComponent<TrailRenderer>();
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
            var otherController = hit.collider.GetComponent<CubeController>();

            if (otherController != null && otherController._isMoving)
            {
                return true; // Diğer küp hareket ediyorsa, engel yok say
            }

                // Eğer aradaki mesafe 1 birim veya daha azsa dur
            if (hit.distance <= 0.51f && hit.collider.gameObject != this.gameObject)
            {
                FlashRed(otherController);

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


    private void FlashRed(CubeController other)
    {
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

        //Çarpışan küpleri kırmızıya flash yap
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
        _moveTween?.Kill(); // Önceki tween'i iptal et
    }
    private void ConfigureTrailRenderer(Color color)
    {
        if (_trail == null) return;

        // 1) Materyali Unlit/Color olarak ayarla, temel rengi ver
        var mat = new Material(Shader.Find("Unlit/Color"));
        mat.SetColor("_Color", color);
        _trail.material = mat;

        // 2) İz süresi: ne kadar uzun bir süre iz kalsın
        _trail.time = 0.45f;

        // 4) Genişlik: çok kalın başlayıp azalarak sönümlensin
        //    startWidth/endWidth birlikte kullanırsan widthCurve’a gerek kalmaz
        _trail.startWidth = 0.46f;   // başlangıçta kalınlık
        _trail.endWidth = 0.0f;   // sonunda inceleyip yok olsun

        // 5) Renk degradeyi ayarla: başta dolgun, sonda saydam
        var grad = new Gradient();
        grad.SetKeys(
            new[]
            {
            new GradientColorKey(color,    0f),
            new GradientColorKey(color * 0.6f, 1f)
            },
            new[]
            {
            new GradientAlphaKey(1f, 0f),
            new GradientAlphaKey(0f, 1f)
            }
        );
        _trail.colorGradient = grad;

        // 6) Kameraya hizalı çizgi (daha temiz görünür)
        _trail.alignment = LineAlignment.View;

        // 7) Hemen temizleyip emit’i aç, böylece hareketin başında eksik iz kalmasın
        _trail.Clear();
        _trail.emitting = false;
        _trail.emitting = true;
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
