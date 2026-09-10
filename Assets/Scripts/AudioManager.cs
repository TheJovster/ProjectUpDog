using UnityEngine;

/// Persistent audio owner. Lives with its AudioSources across scene loads, so
/// music survives menu -> game without restarting.
public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource _soundtrack;
    [SerializeField] private AudioSource _background;

    [SerializeField] private AudioSource _effects;
    [SerializeField] private AudioSource _special;

    [Tooltip("All clips for the prototype.")]
    [SerializeField] private SoundLibrary _sounds;

    private static AudioManager _instance = null;

    /// Clip container, so callers can do audio.PlayEffect(audio.Sounds.pop).
    public SoundLibrary Sounds => _sounds;

    private void Awake()
    {
        // A second copy loading with a new scene would register over the first
        // and orphan the playing sources.
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        ServiceRegistry.Instance.Register(this);
    }

    // --- Soundtrack -----------------------------------------------------

    public void PlaySoundtrack()
    {
        _soundtrack.Play();
    }

    public void StopSoundtrack()
    {
        _soundtrack.Stop();
    }

    public void SetSoundtrack(AudioClip clip)
    {
        _soundtrack.clip = clip;
    }

    // --- Background / environment ---------------------------------------

    public void PlayBackground()
    {
        _background.Play();
    }

    public void StopBackground()
    {
        _background.Stop();
    }

    public void SetBackground(AudioClip clip)
    {
        _background.clip = clip;
    }

    // --- Special --------------------------------------------------------

    public void PlaySpecial()
    {
        _special.Play();
    }

    public void StopSpecial()
    {
        _special.Stop();
    }

    public void SetSpecial(AudioClip clip)
    {
        _special.clip = clip;
    }

    // --- Effects --------------------------------------------------------

    // One-shots layer, so several can overlap on the same source.
    public void PlayEffect(AudioClip clip)
    {
        _effects.PlayOneShot(clip);
    }

    public void PlayEffect(AudioClip clip, float volumeScale)
    {
        _effects.PlayOneShot(clip, volumeScale);
    }
}