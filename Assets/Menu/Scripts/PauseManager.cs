using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance;

    [SerializeField] private GameObject pauseUI;
    [SerializeField] private GameObject settingsPanel;

    private bool isPaused;
    private bool settingsOpen;

    private void Awake()
    {
        Instance = this;
    }

    public void TogglePause(InputAction.CallbackContext ctx)
    {
        isPaused = !isPaused;
        pauseUI.SetActive(isPaused);
        Time.timeScale = isPaused ? 0f : 1f;
    }

    public void Resume()
    {
        isPaused = false;
        pauseUI.SetActive(false);
        Time.timeScale = 1f;
    }

    public void ToggleSettings()
    {
        if (settingsOpen)
        {
            settingsPanel.SetActive(false);
            pauseUI.SetActive(true);
        }
        else
        {
            settingsPanel.SetActive(true);
            pauseUI.SetActive(false);
        }
        settingsOpen = !settingsOpen;
    }

    public void QuitToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
