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
        if (!ctx.performed) return;

        if (settingsOpen) return; // prevent pausing while in settings

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

    public void OpenSettings()
    {
        settingsOpen = true;
        pauseUI.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsOpen = false;
        settingsPanel.SetActive(false);
        pauseUI.SetActive(true);
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
