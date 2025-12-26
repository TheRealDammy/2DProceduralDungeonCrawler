using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Maps : MonoBehaviour
{
    [Header("Map Settings")]
    public int width = 80;
    public int height = 80;
    public float scale = 25f;

    [Tooltip("If true, offset changes every run so the map changes.")]
    public bool randomizeOffset = true;

    public Vector2 baseOffset;
    Vector2 offset;

    [Header("Noise Settings")]
    public Wave[] heightWaves;
    public Wave[] moistureWaves;
    public Wave[] heatWaves;

    float[,] heightMap;
    float[,] moistureMap;
    float[,] heatMap;

    [Header("Biomes")]
    public BiomePresets[] biomes;

    [Header("Tilemaps")]
    public Tilemap groundTilemap;
    public Tilemap pathTilemap;
    public Tilemap wallTilemap;
    public Tilemap propBackTilemap;
    public Tilemap propFrontTilemap;

    [Header("Base Tiles")]
    public TileBase waterTile;
    public TileBase sandTile;

    [Header("Path Tiles")]
    public PathPreset pathPreset;

    [Header("Wall Tiles (border walls are simple)")]
    public TileBase borderWallTile;

    [Header("Props")]
    public PropPreset[] props;

    [Header("Player Spawn")]
    public Transform player;
    public int spawnAttempts = 500;

    // Control thresholds
    [Header("Terrain Thresholds")]
    [Range(0f, 1f)] public float oceanThreshold = 0.10f;
    [Range(0f, 1f)] public float shoreThreshold = 0.15f;

    // Caches
    HashSet<Vector3Int> pathCells = new HashSet<Vector3Int>();

    void Start()
    {
        GenerateMap();
    }

    public void GenerateMap()
    {
        ClearMaps();
        ChooseOffset();
        GenerateNoiseMaps();
        GenerateBaseTerrain();
        GenerateBorderWaterAndWalls();
        GeneratePathsSimple();
        GenerateProps();
        SpawnPlayerRandom();
    }

    void ChooseOffset()
    {
        if (!randomizeOffset)
        {
            offset = baseOffset;
            return;
        }

        // big random offsets -> new map each time
        offset = baseOffset + new Vector2(Random.Range(-10000f, 10000f), Random.Range(-10000f, 10000f));
    }

    void ClearMaps()
    {
        groundTilemap.ClearAllTiles();
        pathTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();
        propBackTilemap.ClearAllTiles();
        propFrontTilemap.ClearAllTiles();
    }

    void GenerateNoiseMaps()
    {
        heightMap = NoiseGenerator.Generate(width, height, heightWaves, scale, offset);
        moistureMap = NoiseGenerator.Generate(width, height, moistureWaves, scale, offset);
        heatMap = NoiseGenerator.Generate(width, height, heatWaves, scale, offset);
    }

    void GenerateBaseTerrain()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                var pos = new Vector3Int(x, y, 0);
                float h = heightMap[x, y];

                if (h < oceanThreshold)
                {
                    groundTilemap.SetTile(pos, waterTile);
                    continue;
                }

                if (h < shoreThreshold)
                {
                    groundTilemap.SetTile(pos, sandTile);
                    continue;
                }

                var biome = GetBiome(h, moistureMap[x, y], heatMap[x, y]);
                groundTilemap.SetTile(pos, biome.GetRandomTile());

                // Optional tint (only if your BiomePresets has a 'tint' field)
                // groundTilemap.SetTileFlags(pos, TileFlags.None);
                // groundTilemap.SetColor(pos, biome.tint);
            }
    }

    void GenerateBorderWaterAndWalls()
    {
        // Force outer ring to be water + wall so player can't leave
        for (int x = 0; x < width; x++)
        {
            SetBorderCell(x, 0);
            SetBorderCell(x, height - 1);
        }

        for (int y = 0; y < height; y++)
        {
            SetBorderCell(0, y);
            SetBorderCell(width - 1, y);
        }
    }

    void SetBorderCell(int x, int y)
    {
        var pos = new Vector3Int(x, y, 0);
        groundTilemap.SetTile(pos, waterTile);
        wallTilemap.SetTile(pos, borderWallTile);
    }

    // Simple landmark-to-landmark paths, but safe and stable
    void GeneratePathsSimple()
    {
        pathCells.Clear();

        // pick 4 landmarks inside the border
        List<Vector2Int> landmarks = GenerateLandmarks(4);

        for (int i = 0; i < landmarks.Count - 1; i++)
            CarvePath(landmarks[i], landmarks[i + 1]);

        // resolve path tiles
        foreach (var cell in pathCells)
            ResolvePathTile(cell);
    }

    void CarvePath(Vector2Int start, Vector2Int end)
    {
        Vector2Int current = start;

        int safety = width * height;
        while (current != end && safety-- > 0)
        {
            var cell = new Vector3Int(current.x, current.y, 0);

            // don't carve through walls/water
            if (wallTilemap.GetTile(cell) == null && groundTilemap.GetTile(cell) != waterTile)
                pathCells.Add(cell);

            // step toward end
            if (Random.value > 0.5f)
                current.x += Mathf.Clamp(end.x - current.x, -1, 1);
            else
                current.y += Mathf.Clamp(end.y - current.y, -1, 1);
        }

        pathCells.Add(new Vector3Int(end.x, end.y, 0));
    }

    void ResolvePathTile(Vector3Int pos)
    {
        if (wallTilemap.GetTile(pos) != null) return;
        if (groundTilemap.GetTile(pos) == waterTile) return;

        bool up = pathCells.Contains(pos + Vector3Int.up);
        bool down = pathCells.Contains(pos + Vector3Int.down);
        bool left = pathCells.Contains(pos + Vector3Int.left);
        bool right = pathCells.Contains(pos + Vector3Int.right);

        int connections = (up ? 1 : 0) + (down ? 1 : 0) + (left ? 1 : 0) + (right ? 1 : 0);

        TileBase tile;
        if (connections == 4) tile = pathPreset.cross;
        else if (connections == 3)
        {
            if (!up) tile = pathPreset.tDown;
            else if (!down) tile = pathPreset.tUp;
            else if (!left) tile = pathPreset.tRight;
            else tile = pathPreset.tLeft;
        }
        else if (connections == 2)
        {
            if (up && down) tile = pathPreset.straightVertical;
            else if (left && right) tile = pathPreset.straightHorizontal;
            else if (up && right) tile = pathPreset.cornerBL;
            else if (up && left) tile = pathPreset.cornerBR;
            else if (down && right) tile = pathPreset.cornerTL;
            else tile = pathPreset.cornerTR;
        }
        else tile = pathPreset.straightHorizontal;

        pathTilemap.SetTile(pos, tile);
    }

    List<Vector2Int> GenerateLandmarks(int count)
    {
        var points = new List<Vector2Int>();
        int attempts = 0;

        while (points.Count < count && attempts++ < 2000)
        {
            int x = Random.Range(2, width - 2);
            int y = Random.Range(2, height - 2);

            var cell = new Vector3Int(x, y, 0);
            if (groundTilemap.GetTile(cell) == waterTile) continue;
            if (wallTilemap.GetTile(cell) != null) continue;

            points.Add(new Vector2Int(x, y));
        }

        return points;
    }

    bool CellOccupied(Vector3Int pos)
    {
        return propBackTilemap.GetTile(pos) != null || propFrontTilemap.GetTile(pos) != null;
    }

    void GenerateProps()
    {
        for (int x = 1; x < width - 1; x++)
            for (int y = 1; y < height - 1; y++)
            {
                var pos = new Vector3Int(x, y, 0);

                if (groundTilemap.GetTile(pos) == waterTile) continue;
                if (wallTilemap.GetTile(pos) != null) continue;
                if (pathTilemap.GetTile(pos) != null) continue;
                if (CellOccupied(pos)) continue;

                foreach (PropPreset prop in props)
                {
                    if (!prop.Matches(heightMap[x, y], moistureMap[x, y], heatMap[x, y])) continue;
                    if (Random.value >= prop.spawnChance) continue;

                    // If you add 'isTall' to PropPreset: choose front/back tilemap
                    // var tm = prop.isTall ? propFrontTilemap : propBackTilemap;

                    // For now, simple: put all props in front map so you see them
                    var tm = propFrontTilemap;

                    tm.SetTile(pos, prop.GetRandomTile());
                    break;
                }
            }
    }

    void SpawnPlayerRandom()
    {
        if (player == null) return;

        for (int i = 0; i < spawnAttempts; i++)
        {
            int x = Random.Range(2, width - 2);
            int y = Random.Range(2, height - 2);

            var cell = new Vector3Int(x, y, 0);

            if (groundTilemap.GetTile(cell) == waterTile) continue;
            if (wallTilemap.GetTile(cell) != null) continue;
            if (pathTilemap.GetTile(cell) != null) continue;
            if (CellOccupied(cell)) continue;

            player.position = groundTilemap.GetCellCenterWorld(cell);
            return;
        }

        player.position = groundTilemap.GetCellCenterWorld(new Vector3Int(width / 2, height / 2, 0));
    }

    BiomePresets GetBiome(float height, float moisture, float heat)
    {
        BiomePresets bestBiome = biomes[0];
        float bestScore = float.MaxValue;

        foreach (var biome in biomes)
        {
            if (!biome.Matches(height, moisture, heat)) continue;

            float score =
                Mathf.Abs(height - biome.minHeight) +
                Mathf.Abs(moisture - biome.minMoisture) +
                Mathf.Abs(heat - biome.minHeat);

            if (score < bestScore)
            {
                bestScore = score;
                bestBiome = biome;
            }
        }
        return bestBiome;
    }
}
