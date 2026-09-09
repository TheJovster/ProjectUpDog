using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

/// Still-frame cutscene: pauses the game, shows a sequence of images, resumes.
/// Slides auto-advance on their duration, or immediately on the advance input.
public class SlideshowPlayer : MonoBehaviour
{
    [System.Serializable]
    public class Slide
    {
        public Sprite image;

        [Tooltip("Seconds before auto-advancing. 0 = wait for input only.")]
        public float duration = 3f;
    }

    [Header("Scene References")]
    [SerializeField] private GameObject _root;      // container enabled during playback
    [SerializeField] private Image _image;

    [Header("Input")]
    [Tooltip("Button action to advance/skip a slide. Optional if every slide has a duration.")]
    [SerializeField] private InputActionReference _advanceAction;

    [Header("Content")]
    [SerializeField] private Slide[] _slides;

    [Tooltip("Play as soon as the scene starts (opening cutscene).")]
    [SerializeField] private bool _playOnStart = false;

    public bool IsPlaying { get; private set; }

    private GameManager _game;
    private InputAction _advance;
    private int _index;
    private float _timer;

    private void Awake()
    {
        if (_advanceAction != null) _advance = _advanceAction.action;
        if (_root != null) _root.SetActive(false);
    }

    private void OnEnable() => _advance?.Enable();
    private void OnDisable() => _advance?.Disable();

    private void Start()
    {
        _game = ServiceRegistry.Instance.Get<GameManager>();
        if (_playOnStart) Play();
    }

    public void Play()
    {
        if (IsPlaying || _slides == null || _slides.Length == 0) return;

        IsPlaying = true;
        _index = -1;
        if (_root != null) _root.SetActive(true);
        if (_game != null) _game.EnterCutscene();
        ShowNext();
    }

    private void Update()
    {
        if (!IsPlaying) return;

        // Input skips the current slide immediately.
        if (_advance != null && _advance.WasPressedThisFrame())
        {
            ShowNext();
            return;
        }

        if (_timer <= 0f) return;   // 0 duration = wait for input
        _timer -= Time.deltaTime;
        if (_timer <= 0f) ShowNext();
    }

    private void ShowNext()
    {
        _index++;
        if (_index >= _slides.Length)
        {
            Stop();
            return;
        }

        Slide slide = _slides[_index];
        if (_image != null) _image.sprite = slide.image;
        _timer = slide.duration;
    }

    public void Stop()
    {
        if (!IsPlaying) return;

        IsPlaying = false;
        if (_root != null) _root.SetActive(false);
        if (_game != null) _game.ExitCutscene();
    }
}