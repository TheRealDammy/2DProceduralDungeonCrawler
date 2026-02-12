using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsUIBinder : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Graphics")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private TMP_Dropdown fpsDropdown;

    [Header("Gameplay")]
    [SerializeField] private Toggle screenShakeToggle;
    [SerializeField] private Toggle damageNumbersToggle;

    [Header("Panels")] 
    [SerializeField] private GameObject graphicsPanel; 
    [SerializeField] private GameObject audioPanel; 
    [SerializeField] private GameObject gameplayPanel;
    [SerializeField] private GameObject contentPanel;
    [SerializeField] private GameObject tabsPanel;

    private bool audioOpened;
    private bool graphicsOpened; 
    private bool gameplayOpened;

    private void Start()
    {
        BindUI();
        LoadCurrentValues();
    }

    private void BindUI()
    {
        masterSlider.onValueChanged.AddListener(SettingsManager.Instance.SetMaster);
        musicSlider.onValueChanged.AddListener(SettingsManager.Instance.SetMusic);
        sfxSlider.onValueChanged.AddListener(SettingsManager.Instance.SetSFX);

        fullscreenToggle.onValueChanged.AddListener(SettingsManager.Instance.SetFullscreen);

        screenShakeToggle.onValueChanged.AddListener(SettingsManager.Instance.ToggleScreenShake);
        damageNumbersToggle.onValueChanged.AddListener(SettingsManager.Instance.ToggleDamageNumbers);

        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        fpsDropdown.onValueChanged.AddListener(OnFPSChanged);
    }

    private void LoadCurrentValues()
    {
        var data = SettingsManager.Instance.GetSettings();

        masterSlider.value = data.masterVolume;
        musicSlider.value = data.musicVolume;
        sfxSlider.value = data.sfxVolume;

        fullscreenToggle.isOn = data.fullscreen;
        screenShakeToggle.isOn = data.screenShake;
        damageNumbersToggle.isOn = data.damageNumbers;

        // FPS dropdown
        switch (data.targetFPS)
        {
            case 30:
                fpsDropdown.value = 0;
                break;
            case 60:
                fpsDropdown.value = 1;
                break;
            default:
                fpsDropdown.value = 2;
                break;
        }

        // Resolution dropdown
        if (data.resolutionWidth == 1280) resolutionDropdown.value = 0;
        else if (data.resolutionWidth == 1600) resolutionDropdown.value = 1;
        else resolutionDropdown.value = 2;
    }


    private void OnResolutionChanged(int index)
    {
        switch (index)
        {
            case 0:
                SettingsManager.Instance.SetResolution(1280, 720);
                break;
            case 1:
                SettingsManager.Instance.SetResolution(1600, 900);
                break;
            case 2:
                SettingsManager.Instance.SetResolution(1920, 1080);
                break;
            case 3: 
                SettingsManager.Instance.SetResolution(2560, 1600); 
                break;
        }
    }

    private void OnFPSChanged(int index)
    {
        switch (index)
        {
            case 0:
                SettingsManager.Instance.SetFPS(30);
                break;
            case 1:
                SettingsManager.Instance.SetFPS(60);
                break;
            case 2:
                SettingsManager.Instance.SetFPS(-1); // Unlimited
                break;
        }
    }
    
    public void ShowAudioPanel() 
    { 
        contentPanel.SetActive(true);
        tabsPanel.SetActive(false);
        audioOpened = true;
        graphicsOpened = false; 
        gameplayOpened = false;
        audioPanel.SetActive(true); 
        graphicsPanel.SetActive(false); 
        gameplayPanel.SetActive(false); 
    } 
    public void ShowGraphicsPanel() 
    {
        contentPanel.SetActive(true);
        tabsPanel.SetActive(false);
        graphicsOpened = true;
        audioOpened = false; 
        gameplayOpened = false;
        audioPanel.SetActive(false); 
        graphicsPanel.SetActive(true); 
        gameplayPanel.SetActive(false); 
    } 
    public void ShowGameplayPanel() 
    {
        contentPanel.SetActive(true);
        tabsPanel.SetActive(false);
        gameplayOpened = true; 
        audioOpened = false; 
        graphicsOpened = false;
        audioPanel.SetActive(false); 
        graphicsPanel.SetActive(false);
        gameplayPanel.SetActive(true); 
    }

    public void ShowTabs() 
    { 
        contentPanel.SetActive(false); 
        tabsPanel.SetActive(true); 
        audioOpened = false; 
        graphicsOpened = false; 
        gameplayOpened = false; 
        audioPanel.SetActive(false); 
        graphicsPanel.SetActive(false); 
        gameplayPanel.SetActive(false); 
    }
}
