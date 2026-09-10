using UnityEngine;

/// End of level (GDD §25). Player enters, scrolling and control stop, win state.
[RequireComponent(typeof(Collider2D))]
public class WinTrigger : MonoBehaviour
{
    [Tooltip("Optional ending cutscene. Plays instead of the win state if set.")]
    [SerializeField] private SlideshowPlayer _endingSlideshow;

    private GameManager _game;
    private bool _triggered;

    private void Start() => _game = ServiceRegistry.Instance.Get<GameManager>();

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_triggered || _game == null) return;
        if (other.GetComponent<PlayerBalloonController>() == null) return;

        _triggered = true;

        if (_endingSlideshow != null)
            _endingSlideshow.Play();
        else
            _game.Win();
    }
}