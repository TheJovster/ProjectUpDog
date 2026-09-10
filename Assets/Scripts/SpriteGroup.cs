using UnityEngine;

/// CanvasGroup equivalent for sprites: one alpha on the parent, multiplied
/// through every child SpriteRenderer. Each child's authored alpha is kept,
/// so a sprite drawn at 0.5 stays half as strong as its neighbours.
[ExecuteAlways]
public class SpriteGroup : MonoBehaviour
{
    [SerializeField, Range(0f, 1f)] private float _alpha = 1f;

    [Tooltip("Include children that are disabled at startup.")]
    [SerializeField] private bool _includeInactive = true;

    private SpriteRenderer[] _renderers;
    private float[] _baseAlphas;
    private float _applied = -1f;

    public float Alpha
    {
        get => _alpha;
        set
        {
            _alpha = Mathf.Clamp01(value);
            Apply();
        }
    }

    private void OnEnable()
    {
        Rebuild();
        Apply(true);
    }

    private void OnValidate() => Apply(true);

    /// Re-scan children. Call after spawning or reparenting sprites at runtime.
    public void Rebuild()
    {
        _renderers = GetComponentsInChildren<SpriteRenderer>(_includeInactive);
        _baseAlphas = new float[_renderers.Length];
        for (int i = 0; i < _renderers.Length; i++)
            _baseAlphas[i] = _renderers[i].color.a;

        _applied = -1f;
    }

    private void Apply(bool force = false)
    {
        if (_renderers == null || _renderers.Length != (_baseAlphas?.Length ?? -1))
            Rebuild();

        // Skip the whole loop when nothing changed — this runs every frame.
        if (!force && Mathf.Approximately(_applied, _alpha)) return;

        for (int i = 0; i < _renderers.Length; i++)
        {
            SpriteRenderer r = _renderers[i];
            if (r == null) continue;

            Color c = r.color;
            c.a = _baseAlphas[i] * _alpha;
            r.color = c;
        }

        _applied = _alpha;
    }
}