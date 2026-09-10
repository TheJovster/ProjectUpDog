using UnityEngine;
using UnityEngine.SceneManagement;

/// Persistent scene loading. Holds no scene references — those break on every
/// load. Scene UI resolves this from the registry and calls in.
public class SceneLoader : MonoBehaviour
{
    [SerializeField] private string _gameSceneName = "GameScene";
    [SerializeField] private string _menuSceneName = "MainMenu";

    private static SceneLoader _instance = null;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        ServiceRegistry.Instance.Register(this);
    }

    public void StartGame()
    {
        SceneManager.LoadScene(_gameSceneName);
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene(_menuSceneName);
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}