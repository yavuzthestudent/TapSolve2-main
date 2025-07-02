using UnityEngine;

public class Background : MonoBehaviour
{
    [SerializeField] private float _speed = 0.5f;
    [SerializeField] private float _maxY = 40;
    [SerializeField] private float _currentPositionY = 0;

    private void FixedUpdate()
    {
        _currentPositionY += _speed;

        this.gameObject.transform.position = new Vector3 (0, _currentPositionY, 0);

        if(_currentPositionY > _maxY)
        {
            this.gameObject.transform.position = new Vector3(0, 0, 0);
            _currentPositionY = 0;
        }

    }
}
