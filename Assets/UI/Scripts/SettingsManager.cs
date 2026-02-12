using UnityEngine;
using UnityEngine.Audio;
using System.IO;

public class SettingsManager : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioMixer mixer;

    private const float MIN_DB = -40f;

    private string savePath;
    private SettingsData settings;

    public static SettingsManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        savePath = Path.Combine(Application.persistentDataPath, "settings.json");

        LoadSettings();
        ApplyAllSettings();
    }

    #region LOAD & SAVE

    private void LoadSettings()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            settings = JsonUtility.FromJson<SettingsData>(json);
        }
        else
        {
            settings = new SettingsData();
            SaveSettings();
        }
    }

    private void SaveSettings()
    {
        string json = JsonUtility.ToJson(settings, true);
        File.WriteAllText(savePath, json);
    }

    public SettingsData GetSettings()
    {
        return settings;
    }

    #endregion

    #region APPLY SETTINGS

    private void ApplyAllSettings()
    {
        ApplyAudio();
        ApplyGraphics();
        ApplyGameplay();
    }

    private void ApplyAudio()
    {
        SetVolume("MasterVol", settings.masterVolume);
        SetVolume("MusicVol", settings.musicVolume);
        SetVolume("SFXVol", settings.sfxVolume);
    }

    private void ApplyGraphics()
    {
        Screen.fullScreen = settings.fullscreen;

        Application.targetFrameRate = settings.targetFPS;

        Screen.SetResolution(
            settings.resolutionWidth,
            settings.resolutionHeight,
            settings.fullscreen
        );
    }

    private void ApplyGameplay()
    {
        // Hook into other systems
        CameraShake.enabledGlobal = settings.screenShake;
        DamageNumberSpawner.enabledGlobal = settings.damageNumbers;
    }

    #endregion

    #region AUDIO

    public void SetMaster(float value)
    {
        settings.masterVolume = value;
        SetVolume("MasterVol", value);
        SaveSettings();
    }

    public void SetMusic(float value)
    {
        settings.musicVolume = value;
        SetVolume("MusicVol", value);
        SaveSettings();
    }

    public void SetSFX(float value)
    {
        settings.sfxVolume = value;
        SetVolume("SFXVol", value);
        SaveSettings();
    }

    private void SetVolume(string param, float value)
    {
        float dB = value <= 0.001f
            ? MIN_DB
            : Mathf.Log10(value) * 20f;

        mixer.SetFloat(param, dB);
    }

    #endregion

    #region GRAPHICS

    public void SetFullscreen(bool value)
    {
        settings.fullscreen = value;
        Screen.fullScreen = value;
        SaveSettings();
    }

    public void SetFPS(int fps)
    {
        settings.targetFPS = fps;
        Application.targetFrameRate = fps;
        SaveSettings();
    }

    public void SetResolution(int width, int height)
    {
        settings.resolutionWidth = width;
        settings.resolutionHeight = height;

        Screen.SetResolution(width, height, settings.fullscreen);
        SaveSettings();
    }

    #endregion

    #region GAMEPLAY

    public void ToggleScreenShake(bool value)
    {
        settings.screenShake = value;
        CameraShake.enabledGlobal = value;
        SaveSettings();
    }

    public void ToggleDamageNumbers(bool value)
    {
        settings.damageNumbers = value;
        DamageNumberSpawner.enabledGlobal = value;
        SaveSettings();
    }

    #endregion
}
