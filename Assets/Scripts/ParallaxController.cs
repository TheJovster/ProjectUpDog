using UnityEngine;

/// Fixed-camera vertical parallax (GDD §2, §9–§14). Each layer is kept centered
/// on the camera so it always fills the view, and its content scrolls downward
/// at speed * factor, wrapping on a loop height for seamless endless depth.
///
/// FACTOR CONVENTION: higher = nearer = faster. Foreground ~1.0, near ~0.75,
/// mid ~0.4–0.6, far ~0.1–0.3, fully static = 0. (This is NOT the camera-follow
/// convention where 1 means "static/far" — near is high here.)
public class ParallaxController : MonoBehaviour
{
    [System.Serializable]
    public class Layer
    {
        public Transform transform;

        [Tooltip("Higher = nearer = faster. Foreground ~1, near ~0.75, mid ~0.5, far ~0.15, static = 0.")]
        public float factor = 0.5f;

        [Tooltip("Vertical distance after which the art repeats. 0 = auto-read the SpriteRenderer height.")]
        public float loopHeight = 0f;

        [System.NonSerialized] public Vector3 offset;   // placement relative to the camera
        [System.NonSerialized] public float scrolled;
        [System.NonSerialized] public float resolvedLoop;
    }

    [SerializeField] private Layer[] _layers;

    [Tooltip("Neutral downward scroll speed (units/sec) representing base ascent.")]
    [SerializeField] private float _baseScrollSpeed = 6f;

    [Tooltip("Extra scroll speed added per unit of player upward input (GDD §12).")]
    [SerializeField] private float _playerInfluence = 4f;

    [Tooltip("Optional. When set, holding Up speeds the scroll for perceived faster ascent.")]
    [SerializeField] private PlayerBalloonController _player;

    [Tooltip("Leave empty to use Camera.main.")]
    [SerializeField] private Camera _camera;

    private void Awake()
    {
        if (_camera == null) _camera = Camera.main;
        if (_layers == null || _camera == null) return;

        Vector3 camPos = _camera.transform.position;
        for (int i = 0; i < _layers.Length; i++)
        {
            Layer layer = _layers[i];
            if (layer.transform == null) continue;

            // Preserve where each layer was placed relative to the camera.
            layer.offset = layer.transform.position - camPos;
            layer.scrolled = 0f;

            // Explicit loop height wins; otherwise fall back to the sprite's height.
            layer.resolvedLoop = layer.loopHeight;
            if (layer.resolvedLoop <= 0f && layer.transform.TryGetComponent(out SpriteRenderer sprite))
                layer.resolvedLoop = sprite.bounds.size.y;
        }
    }

    private void LateUpdate()
    {
        if (_layers == null || _camera == null) return;

        // effectiveScrollSpeed = base + player upward contribution (GDD §12).
        float scrollSpeed = _baseScrollSpeed;
        if (_player != null)
            scrollSpeed += _player.UpwardContribution * _playerInfluence;

        float step = scrollSpeed * Time.deltaTime;
        Vector3 camPos = _camera.transform.position;

        for (int i = 0; i < _layers.Length; i++)
        {
            Layer layer = _layers[i];
            if (layer.transform == null) continue;

            layer.scrolled += step * layer.factor;

            // Wrap within one loop so a tiled layer never runs out.
            if (layer.resolvedLoop > 0f)
            {
                layer.scrolled %= layer.resolvedLoop;
                if (layer.scrolled < 0f) layer.scrolled += layer.resolvedLoop;
            }

            // Re-center on the camera (keeping the authored offset + Z), then
            // scroll the content downward.
            Vector3 pos = camPos + layer.offset;
            pos.y -= layer.scrolled;
            layer.transform.position = pos;
        }
    }
}