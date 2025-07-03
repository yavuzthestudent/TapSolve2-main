using UnityEngine;
using UnityEngine.EventSystems;

public class ClickManager : MonoBehaviour
{
    private Camera _camera;

    private void Awake()
    {
        _camera = Camera.main;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // E�er imle� herhangi bir UI ��esinin (buton, panel vb.) �zerindeyse
            if (EventSystem.current.IsPointerOverGameObject())
                return;  // 3D sahneye t�klamay� iptal et

            // De�ilse sahneye raycast at
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit) &&
                hit.collider.TryGetComponent<IClickable>(out var clickable))
            {
                clickable.OnClick();
            }
        }
    }
}
