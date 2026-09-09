using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    [SerializeField] private WorldScroller _worldScroller;
    [SerializeField] private float _factor = 0.5f;
    [SerializeField] private float _height = 10f;  // sprite height

    private float _startY;

    private void Awake() => _startY = transform.position.y;

    private void LateUpdate()
    {
        float offset = Mathf.Repeat(_worldScroller.Distance * _factor, _height);
        Vector3 p = transform.position;
        p.y = _startY - offset;
        transform.position = p;
    }
}