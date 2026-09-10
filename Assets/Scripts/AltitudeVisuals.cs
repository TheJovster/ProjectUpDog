using UnityEngine;

/// Drives sky colour and cloud/star alpha by altitude (GDD §13).
///
/// Everything here is a pure function of WorldScroller.Distance, recomputed
/// every frame. That means checkpoint restore needs no visual state: rewinding
/// distance rewinds the sky and every alpha exactly.
public class AltitudeVisuals : MonoBehaviour
{
    /// One fading sprite: transparent, fades in over the in-band, holds full
    /// alpha, then fades out over the out-band.
    [System.Serializable]
    public class FadeLayer
    {
        [Tooltip("Single sprite. Leave empty if using a group instead.")]
        public SpriteRenderer renderer;

        [Tooltip("Parent of many sprites (clouds, crowds, props). Leave empty if using a single renderer.")]
        public SpriteGroup group;

        [Tooltip("Distance where this starts becoming visible.")]
        public float fadeInStart = 0f;

        [Tooltip("Distance where it reaches full alpha.")]
        public float fadeInEnd = 50f;

        [Tooltip("Distance where it starts disappearing again.")]
        public float fadeOutStart = 300f;

        [Tooltip("Distance where it is fully gone. Leave both fadeOut values at 0 to never fade out.")]
        public float fadeOutEnd = 400f;

        [Tooltip("Alpha at full visibility.")]
        [Range(0f, 1f)] public float maxAlpha = 1f;
    }

    [Header("References")]
    [SerializeField] private WorldScroller _worldScroller;

    [Header("Sky")]
    [SerializeField] private SpriteRenderer _sky;
    [SerializeField] private Color _lowColor = new Color(0.55f, 0.75f, 0.95f, 1f);
    [SerializeField] private Color _highColor = Color.black;

    [Tooltip("Distance where the sky begins darkening.")]
    [SerializeField] private float _skyStart = 100f;

    [Tooltip("Distance where the sky is fully the high colour.")]
    [SerializeField] private float _skyEnd = 400f;

    [Header("Clouds and Stars")]
    [Tooltip("Two cloud layers and two star layers.")]
    [SerializeField] private FadeLayer[] _layers;

    private void Awake()
    {
        if (_worldScroller == null)
            Debug.LogError("[AltitudeVisuals] WorldScroller not assigned — distance reads 0.", this);

        if (_layers == null) return;

        for (int i = 0; i < _layers.Length; i++)
        {
            FadeLayer layer = _layers[i];
            if (layer.renderer == null && layer.group == null)
            {
                Debug.LogError($"[AltitudeVisuals] Layer {i} has no renderer or group.", this);
                continue;
            }

            // Inspector-added array elements zero-fill and ignore field defaults,
            // which silently pins alpha to 0.
            string label = layer.renderer != null ? layer.renderer.name : layer.group.name;
            if (layer.maxAlpha <= 0f)
                Debug.LogWarning($"[AltitudeVisuals] '{label}' maxAlpha is 0 — it will never be visible.", this);
            if (layer.fadeInEnd <= layer.fadeInStart)
                Debug.LogWarning($"[AltitudeVisuals] '{label}' has no fade-in band (start {layer.fadeInStart}, end {layer.fadeInEnd}) — it will pop instead of fading.", this);
        }
    }

    private void LateUpdate()
    {
        float distance = _worldScroller != null ? _worldScroller.Distance : 0f;
        Apply(distance);
    }

    private void Apply(float distance)
    {
        if (_sky != null)
        {
            float t = Progress(distance, _skyStart, _skyEnd);
            // Smoothed so the darkening eases in rather than starting abruptly.
            _sky.color = Color.Lerp(_lowColor, _highColor, MathUtility.SmoothStep(t));
        }

        if (_layers == null) return;

        for (int i = 0; i < _layers.Length; i++)
        {
            FadeLayer layer = _layers[i];
            if (layer.renderer == null && layer.group == null) continue;

            float alpha = layer.maxAlpha * Progress(distance, layer.fadeInStart, layer.fadeInEnd);

            // fadeOutEnd of 0 means "never fades out" — stars stay up once risen.
            if (layer.fadeOutEnd > 0f)
                alpha *= 1f - Progress(distance, layer.fadeOutStart, layer.fadeOutEnd);

            if (layer.group != null)
            {
                layer.group.Alpha = alpha;
            }
            else
            {
                Color c = layer.renderer.color;
                c.a = alpha;
                layer.renderer.color = c;
            }
        }
    }

    /// 0 before start, 1 after end, linear between. Guards a zero-width band.
    private static float Progress(float value, float start, float end)
    {
        if (end <= start) return value >= end ? 1f : 0f;
        return Mathf.Clamp01((value - start) / (end - start));
    }
}