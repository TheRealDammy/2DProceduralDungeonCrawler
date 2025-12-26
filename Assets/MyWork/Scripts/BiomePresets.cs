using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "BiomePreset", menuName = "Procedural/Biome Preset")]
public class BiomePresets : ScriptableObject
{
    public TileBase[] tiles;

    [Header("Conditions")]
    public float minHeight;
    public float minMoisture;
    public float minHeat;
    public Color tint = Color.white;     // main biome tint
    public float blendSharpness = 6f;    // higher = sharper borders, lower = smoother


    public TileBase GetRandomTile()
    {
        return tiles[Random.Range(0, tiles.Length)];
    }

    public bool Matches(float height, float moisture, float heat)
    {
        return height >= minHeight && moisture >= minMoisture && heat >= minHeat;
    }
}