using UnityEngine;

/// Drives the UPDOG/SpriteBlur shader per-sprite. Uses a MaterialPropertyBlock
/// so every sprite can be tuned independently without instancing materials.
[RequireComponent(typeof(SpriteRenderer))]
[ExecuteAlways]
public class SpriteBlur : MonoBehaviour
{
    [Tooltip("Blur spread in texels. 0 = sharp.")]
    [SerializeField, Range(0f, 16f)] private float _radius = 2f;

    [Tooltip("Blend between the original sprite (0) and the blurred result (1).")]
    [SerializeField, Range(0f, 1f)] private float _strength = 1f;

    private SpriteRenderer _renderer;
    private MaterialPropertyBlock _block;

    private static readonly int RadiusId = Shader.PropertyToID("_BlurRadius");
    private static readonly int StrengthId = Shader.PropertyToID("_Strength");

    public float Radius
    {
        get => _radius;
        set { _radius = value; Apply(); }
    }

    public float Strength
    {
        get => _strength;
        set { _strength = value; Apply(); }
    }

    private void OnEnable()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _block = new MaterialPropertyBlock();
        Apply();
    }

    private void OnValidate() => Apply();

    private void Apply()
    {
        if (_renderer == null || _block == null) return;

        _renderer.GetPropertyBlock(_block);
        _block.SetFloat(RadiusId, _radius);
        _block.SetFloat(StrengthId, _strength);
        _renderer.SetPropertyBlock(_block);
    }
}
