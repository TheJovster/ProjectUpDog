using UnityEngine;
using UnityEngine.UI;

/// Menu-scene UI. Lives and dies with the scene, so it owns the button
/// references and calls into the persistent SceneLoader.
public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button _startGameButton;
    [SerializeField] private Button _quitGameButton;

    private SceneLoader _loader;

    private void Start()
    {
        _loader = ServiceRegistry.Instance.Get<SceneLoader>();
        if (_loader == null)
        {
            Debug.LogError("[MainMenuUI] No SceneLoader registered.", this);
            return;
        }

        if (_startGameButton != null) _startGameButton.onClick.AddListener(_loader.StartGame);
        if (_quitGameButton != null) _quitGameButton.onClick.AddListener(_loader.QuitGame);
    }
}