using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Procedural/Prop Preset")]
public class PropPreset : ScriptableObject
{
    public TileBase[] tiles;

    [Range(0f, 1f)]
    public float spawnChance = 0.05f;

    public bool isTall = false; // <-- ADD THIS

    [Header("Conditions")]
    public float minHeight;
    public float minMoisture;
    public float minHeat;

    public TileBase GetRandomTile()
    {
        return tiles[Random.Range(0, tiles.Length)];
    }

    public bool Matches(float height, float moisture, float heat)
    {
        return height >= minHeight &&
               moisture >= minMoisture &&
               heat >= minHeat;
    }
}
