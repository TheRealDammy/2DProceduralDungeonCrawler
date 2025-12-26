using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Procedural/Wall Preset")]
public class WallPreset : ScriptableObject
{
    public TileBase tl, tr, bl, br;
    public TileBase top, bottom, left, right;
    public TileBase single;
}
