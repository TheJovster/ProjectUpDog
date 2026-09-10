using UnityEngine;
using UnityEngine.UI;

/// Lives display. Images are disabled from the highest index down as health
/// drops, so index 0 is the last one standing.
public class LivesCounter : MonoBehaviour
{
    [Tooltip("One image per life, ordered low to high.")]
    [SerializeField] private Image[] _lifeImages;

    private GameManager _game;
    private int _shown = -1;

    private void Start()
    {
        _game = ServiceRegistry.Instance.Get<GameManager>();
        if (_game == null)
            Debug.LogError("[LivesCounter] No GameManager registered.", this);
    }

    private void Update()
    {
        if (_game == null) return;

        // Health changes rarely; skip the loop unless it moved.
        if (_game.Health == _shown) return;

        _shown = _game.Health;
        for (int i = 0; i < _lifeImages.Length; i++)
        {
            if (_lifeImages[i] == null) continue;
            _lifeImages[i].enabled = i < _shown;
        }
    }
}