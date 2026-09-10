using System.Collections;
using UnityEngine;

/// Balloon pop feedback (GDD §20). Separate from PlayerBalloonController
/// because GameManager disables that on death, which would kill the coroutine.
public class PlayerDeathEffect : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Player's own sprite, hidden once the pop finishes.")]
    [SerializeField] private SpriteRenderer _playerRenderer;

    [Tooltip("Pop burst sprite, childed to the player. Hidden until death.")]
    [SerializeField] private SpriteRenderer _popRenderer;

    [Header("Pop")]
    [SerializeField] private float _expandDuration = 0.25f;
    [SerializeField] private float _startScale = 0.2f;
    [SerializeField] private float _endScale = 2f;

    private AudioManager _audio;
    private Coroutine _routine;

    private void Start()
    {
        _audio = ServiceRegistry.Instance.Get<AudioManager>();
        if (_popRenderer != null) _popRenderer.enabled = false;
    }

    public void Play()
    {
        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(PopRoutine());
    }

    private IEnumerator PopRoutine()
    {
        if (_audio != null && _audio.Sounds != null)
            _audio.PlayEffect(_audio.Sounds.death);

        if (_popRenderer != null)
        {
            _popRenderer.enabled = true;
            _popRenderer.transform.localScale = Vector3.one * _startScale;
        }

        float elapsed = 0f;
        while (elapsed < _expandDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / _expandDuration);

            // Fast out, settling — the burst should snap open, not ramp up.
            float scale = Mathf.Lerp(_startScale, _endScale, MathUtility.EaseOut(t));
            if (_popRenderer != null)
                _popRenderer.transform.localScale = Vector3.one * scale;

            yield return null;
        }

        if (_playerRenderer != null) _playerRenderer.enabled = false;
        if (_popRenderer != null) _popRenderer.enabled = false;

        _routine = null;
    }

    /// Restore visuals for respawn.
    public void ResetVisuals()
    {
        if (_routine != null)
        {
            StopCoroutine(_routine);
            _routine = null;
        }

        if (_playerRenderer != null) _playerRenderer.enabled = true;
        if (_popRenderer != null)
        {
            _popRenderer.enabled = false;
            _popRenderer.transform.localScale = Vector3.one * _startScale;
        }
    }
}