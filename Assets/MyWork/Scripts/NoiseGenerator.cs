using UnityEngine;

public class NoiseGenerator
{
    public static float[,] Generate(int width, int height, Wave[] waves, float scale, Vector2 offset)
    {
        float[,] noiseMap = new float[width, height];

        if (scale <= 0f) scale = 0.0001f;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float sampleX = x / scale + offset.x;
                float sampleY = y / scale + offset.y;

                float noiseSum = 0f;
                float normalization = 0f;

                foreach (Wave wave in waves)
                {
                    noiseSum += wave.amplitude * Mathf.PerlinNoise(
                        sampleX * wave.frequency + wave.seed,
                        sampleY * wave.frequency + wave.seed
                    );
                    normalization += wave.amplitude;
                }

                noiseMap[x, y] = (normalization > 0f) ? (noiseSum / normalization) : 0f;
            }
        }

        return noiseMap;
    }
}

[System.Serializable]
public class Wave
{
    public float seed;
    public float frequency;
    public float amplitude;
}
