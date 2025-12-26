using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Procedural/Path Preset")]
public class PathPreset : ScriptableObject
{
    [Header("Straight")]
    public TileBase straightHorizontal;
    public TileBase straightVertical;

    [Header("Corners")]
    public TileBase cornerTL;
    public TileBase cornerTR;
    public TileBase cornerBL;
    public TileBase cornerBR;

    [Header("Junctions")]
    public TileBase tUp;
    public TileBase tDown;
    public TileBase tLeft;
    public TileBase tRight;

    [Header("Cross")]
    public TileBase cross;

    public TileBase Pick(TileBase[] arr) => (arr != null && arr.Length > 0) ? arr[Random.Range(0, arr.Length)] : null;
}
