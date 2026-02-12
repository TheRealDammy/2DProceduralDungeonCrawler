using System;

[Serializable]
public class SettingsData
{
    // Audio
    public float masterVolume = 1f;
    public float musicVolume = 1f;
    public float sfxVolume = 1f;

    // Graphics
    public bool fullscreen = true;
    public int targetFPS = 60;

    // Gameplay
    public bool screenShake = true;
    public bool damageNumbers = true;

    // Resolution
    public int resolutionWidth = 1920;
    public int resolutionHeight = 1080;
}
