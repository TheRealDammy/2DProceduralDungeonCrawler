using UnityEngine;
using UnityEngine.Audio;

public class SettingsManager : MonoBehaviour
{
    [SerializeField] private AudioMixer mixer;

    private const string MASTER = "MasterVol";
    private const string MUSIC = "MusicVol";
    private const string SFX = "SFXVol";

    private const float MIN_DB = -40f;

    private void Start()
    {
        SetMaster(PlayerPrefs.GetFloat(MASTER, 1f));
        SetMusic(PlayerPrefs.GetFloat(MUSIC, 1f));
        SetSFX(PlayerPrefs.GetFloat(SFX, 1f));
    }

    public void SetMaster(float value)
    {
        SetVolume(MASTER, value);
    }

    public void SetMusic(float value)
    {
        SetVolume(MUSIC, value);
    }

    public void SetSFX(float value)
    {
        SetVolume(SFX, value);
    }

    private void SetVolume(string param, float value)
    {
        float dB = value <= 0.001f
            ? MIN_DB
            : Mathf.Log10(value) * 20f;

        mixer.SetFloat(param, dB);
        PlayerPrefs.SetFloat(param, value);

    }

    public void Back()
    {
        gameObject.SetActive(false);
        PauseManager.Instance.ToggleSettings();
    }
}
