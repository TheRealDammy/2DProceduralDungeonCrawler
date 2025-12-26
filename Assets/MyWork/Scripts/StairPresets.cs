using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "StairPresets", menuName = "Procedural/StairPresets")]
public class StairPresets : ScriptableObject
{
    [Header("Stairs that go UP to the right")]
    public TileBase[] bottomRight;
    public TileBase[] middleRight;
    public TileBase[] topRight;

    [Header("Stairs that go UP to the left")]
    public TileBase[] bottomLeft;
    public TileBase[] middleLeft;
    public TileBase[] topLeft;

    public int height = 3;

    public TileBase Pick(TileBase[] arr) => (arr == null || arr.Length == 0) ? null : arr[Random.Range(0, arr.Length)];
}
