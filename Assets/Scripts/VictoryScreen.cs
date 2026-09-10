using UnityEngine;
using UnityEngine.UI;

/// Victory UI (GDD §25). Shown when GameManager enters Win, offers a way back
/// to the main menu.
public class VictoryScreen : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject _root;
    [SerializeField] private Button _mainMenuButton;

    private GameManager _game;
    private SceneLoader _loader;
    private bool _shown;

    private void Awake()
    {
        if (_root != null) _root.SetActive(false);
    }

    private void Start()
    {
        _game = ServiceRegistry.Instance.Get<GameManager>();
        _loader = ServiceRegistry.Instance.Get<SceneLoader>();
        if (_mainMenuButton != null)
            _mainMenuButton.onClick.AddListener(ReturnToMenu);
    }

    private void Update()
    {
        if (_shown || _game == null) return;
        if (_game.Current != GameManager.State.Win) return;

        Show();
    }

    private void Show()
    {
        _shown = true;
        if (_root != null) _root.SetActive(true);
    }

    private void ReturnToMenu()
    {
        if (_loader != null) _loader.ReturnToMenu();
    }
}