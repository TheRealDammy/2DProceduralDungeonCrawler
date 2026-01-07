using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileMapGenerator : MonoBehaviour
{
    [SerializeField] private Tilemap floorTilemap;
    [SerializeField] private Tilemap wallTilemap;
    [SerializeField] private TileBase[] floorTile;
    [SerializeField] private TileBase[] wallTop;
    [SerializeField] private TileBase[] wallBottom;
    [SerializeField] private TileBase[] wallLeft;
    [SerializeField] private TileBase[] wallRight;
    [SerializeField] private TileBase[] wallFull;
    [SerializeField] private TileBase[] wallInnerCornerDownLeft;
    [SerializeField] private TileBase[] wallInnerCornerDownRight;
    [SerializeField] private TileBase[] wallDiagonalCornerDownLeft;
    [SerializeField] private TileBase[] wallDiagonalCornerDownRight;
    [SerializeField] private TileBase[] wallDiagonalCornerUpLeft;
    [SerializeField] private TileBase[] wallDiagonalCornerUpRight;

    public void PaintFloorTiles(IEnumerable<Vector2Int> floorPositions)
    {
        PaintTiles(floorPositions, floorTilemap, floorTile);
         
    }
    private void PaintTiles(IEnumerable<Vector2Int> positions, Tilemap tilemap, TileBase[] tile)
    {
        foreach (var position in positions)
        {
            PaintSingleFloorTile(tilemap, tile, position);
        }
    }

    private void PaintSingleFloorTile(Tilemap tilemap, TileBase[] tile, Vector2Int position)
    {
        var tilePosition = tilemap.WorldToCell((Vector3Int)position);
        var randomFloorTile = floorTile[Random.Range(0, floorTile.Length)];

        tilemap.SetTile(tilePosition, randomFloorTile);      
    }
    private void PaintSingleWallTile(Tilemap tilemap, TileBase[] tile, Vector2Int position)
    {
        var tilePosition = tilemap.WorldToCell((Vector3Int)position);
        var randomWallTile = tile[Random.Range(0, tile.Length)];
        tilemap.SetTile(tilePosition, randomWallTile);
    }

    public void Clear()
    {
        wallTilemap.ClearAllTiles();
        floorTilemap.ClearAllTiles();
    }

    internal void PaintSingleWall(Vector2Int position, string binaryType)
    {
        int typeAsInt = System.Convert.ToInt32(binaryType, 2);
        TileBase[] tile = null;
        if (WallTypesHelper.wallTop.Contains(typeAsInt))
        {
            tile = wallTop;
        }
        else if (WallTypesHelper.wallSideLeft.Contains(typeAsInt))
        {
            tile = wallLeft;
        }
        else if (WallTypesHelper.wallSideRight.Contains(typeAsInt))
        {
            tile = wallRight;
        }
        else if (WallTypesHelper.wallBottm.Contains(typeAsInt))
        {
            tile = wallBottom;
        }
        else if (WallTypesHelper.wallFull.Contains(typeAsInt))
        {
            tile = wallFull;
        }
        if (tile != null)
        {
            PaintSingleWallTile(wallTilemap, tile, position);
        }
        
    }

    internal void PaintSingleCornerWall(Vector2Int position, string binaryType)
    {
        int TypeAsInt = System.Convert.ToInt32(binaryType, 2);
        TileBase[] tile = null;
        if (WallTypesHelper.wallInnerCornerDownLeft.Contains(TypeAsInt))
        {
            tile = wallInnerCornerDownLeft;
        }
        else if (WallTypesHelper.wallInnerCornerDownRight.Contains(TypeAsInt))
        {
            tile = wallInnerCornerDownRight;
        }
        else if (WallTypesHelper.wallDiagonalCornerDownLeft.Contains(TypeAsInt))
        {
            tile = wallDiagonalCornerDownLeft;
        }
        else if (WallTypesHelper.wallDiagonalCornerDownRight.Contains(TypeAsInt))
        {
            tile = wallDiagonalCornerDownRight;
        }
        else if (WallTypesHelper.wallDiagonalCornerUpLeft.Contains(TypeAsInt))
        {
            tile = wallDiagonalCornerUpLeft;
        }
        else if (WallTypesHelper.wallDiagonalCornerUpRight.Contains(TypeAsInt))
        {
            tile = wallDiagonalCornerUpRight;
        }
        else if (WallTypesHelper.wallFullEightDirections.Contains(TypeAsInt))
        {
            tile = wallFull;
        }
        else if (WallTypesHelper.wallBottmEightDirections.Contains(TypeAsInt))
        {
            tile = wallBottom;
        }
        if (tile != null)
        {
            PaintSingleWallTile(wallTilemap, tile, position);
        }
    }
}
