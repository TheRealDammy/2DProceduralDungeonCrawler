using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Maps : MonoBehaviour
{
    [Header("Map Settings")]
    public int width = 80;
    public int height = 60;
    public float scale = 12f;

    [Header("Randomness")]
    public bool randomizeEachRun = true;
    public Vector2 baseOffset;
    public float offsetJitter = 9999f;

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

    [Header("Thresholds")]
    [Range(0f, 1f)] public float waterHeight = 0.10f;
    [Range(0f, 1f)] public float sandHeight = 0.15f;

    [Header("Edge Water (removes ponds)")]
    [Tooltip("How many tiles from border are forced lower to create an 'ocean outside' feel.")]
    public int edgeWaterBand = 6;
    [Tooltip("If true, interior water is removed (no ponds/lakes inside).")]
    public bool removeInteriorPonds = true;

    [Header("Paths")]
    public PathPreset pathPreset;
    public int landmarkCount = 6;

    [Header("Walls")]
    public WallPreset wallPreset;
    [Tooltip("How many entrances in the border walls")]
    public int entranceCount = 2;
    [Tooltip("Entrance width in tiles")]
    public int entranceWidth = 2;

    [Header("Stairs (at entrances)")]
    public StairPresets stairPresets;

    [Header("Props")]
    public PropPreset[] props;
    [Range(0f, 1f)] public float extraPropClearChance = 1f; // 1 = always enforce clear, keep

    [Header("Player Spawn")]
    public Transform player;
    public string playerTag = "Player";
    public int spawnAttempts = 500;

    // internal
    HashSet<Vector3Int> pathCells = new HashSet<Vector3Int>();
    HashSet<Vector3Int> wallCells = new HashSet<Vector3Int>();

    void Start()
    {
        GenerateMap();
    }

    public void GenerateMap()
    {
        EnsurePlayerReference();

        ClearMaps();

        Vector2 offset = baseOffset;
        if (randomizeEachRun)
        {
            offset += new Vector2(
                Random.Range(-offsetJitter, offsetJitter),
                Random.Range(-offsetJitter, offsetJitter)
            );
        }

        GenerateNoiseMaps(offset);
        ApplyEdgeWaterFalloff();     // creates ocean outside feel + helps remove ponds
        GenerateBaseTerrain();       // includes biome tint blending

        GenerateBorderWallsWithEntrances();  // border walls + entrance gaps
        ResolveAllWalls();                  // uses tl/tr/bl/br etc.

        GeneratePaths();                     // avoids walls
        ResolveAllPaths();                   // uses pathPreset

        PlaceEntranceStairs();               // multi-tile stairs at the entrances

        GenerateProps();                     // never on water/sand/walls/paths/props

        SpawnPlayerRandom();
    }

    void EnsurePlayerReference()
    {
        if (player != null) return;

        GameObject p = GameObject.FindGameObjectWithTag(playerTag);
        if (p != null) player = p.transform;
    }

    void ClearMaps()
    {
        if (groundTilemap) groundTilemap.ClearAllTiles();
        if (pathTilemap) pathTilemap.ClearAllTiles();
        if (wallTilemap) wallTilemap.ClearAllTiles();
        if (propBackTilemap) propBackTilemap.ClearAllTiles();
        if (propFrontTilemap) propFrontTilemap.ClearAllTiles();

        pathCells.Clear();
        wallCells.Clear();
    }

    void GenerateNoiseMaps(Vector2 offset)
    {
        heightMap = NoiseGenerator.Generate(width, height, heightWaves, scale, offset);
        moistureMap = NoiseGenerator.Generate(width, height, moistureWaves, scale, offset);
        heatMap = NoiseGenerator.Generate(width, height, heatWaves, scale, offset);
    }

    // Force the outside border band to be lower => ocean around the playable area.
    void ApplyEdgeWaterFalloff()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int d = DistanceToEdge(x, y);
                if (d < edgeWaterBand)
                {
                    // push height down near edges
                    float t = Mathf.InverseLerp(edgeWaterBand, 0, d); // 0 at band edge, 1 at border
                    heightMap[x, y] = Mathf.Lerp(heightMap[x, y], 0f, t);
                }
            }
        }
    }

    int DistanceToEdge(int x, int y)
    {
        int left = x;
        int right = (width - 1) - x;
        int bottom = y;
        int top = (height - 1) - y;
        return Mathf.Min(Mathf.Min(left, right), Mathf.Min(bottom, top));
    }

    void GenerateBaseTerrain()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float h = heightMap[x, y];
                Vector3Int pos = new Vector3Int(x, y, 0);

                // Remove interior ponds: if it's low enough for water but not near the edge band, lift it.
                if (removeInteriorPonds)
                {
                    int d = DistanceToEdge(x, y);
                    if (d >= edgeWaterBand && h < waterHeight)
                        h = sandHeight + 0.001f;
                }

                // Water
                if (h < waterHeight)
                {
                    groundTilemap.SetTile(pos, waterTile);
                    continue;
                }

                // Sand shore
                if (h < sandHeight)
                {
                    groundTilemap.SetTile(pos, sandTile);
                    continue;
                }

                // Biomes with soft blending via tint
                float m = moistureMap[x, y];
                float heat = heatMap[x, y];

                // pick best + second best for tint blending
                GetBestTwoBiomes(h, m, heat, out BiomePresets best, out BiomePresets second, out float bestScore, out float secondScore);

                TileBase tile = best.GetRandomTile();
                groundTilemap.SetTile(pos, tile);

                // Blend the tint between best and second best
                // weight gets sharper with blendSharpness
                float w = 1f;
                if (second != null && secondScore < float.MaxValue)
                {
                    float diff = Mathf.Max(0.0001f, secondScore - bestScore);
                    w = Mathf.Clamp01(Mathf.Pow(1f / (1f + diff), best.blendSharpness));
                }

                Color blended = (second == null) ? best.tint : Color.Lerp(second.tint, best.tint, w);

                groundTilemap.SetTileFlags(pos, TileFlags.None);
                groundTilemap.SetColor(pos, blended);
            }
        }
    }

    void GetBestTwoBiomes(float h, float m, float heat,
        out BiomePresets best, out BiomePresets second,
        out float bestScore, out float secondScore)
    {
        best = null;
        second = null;
        bestScore = float.MaxValue;
        secondScore = float.MaxValue;

        foreach (var b in biomes)
        {
            if (b == null) continue;
            if (!b.Matches(h, m, heat)) continue;

            float score =
                Mathf.Abs(h - b.minHeight) +
                Mathf.Abs(m - b.minMoisture) +
                Mathf.Abs(heat - b.minHeat);

            if (score < bestScore)
            {
                second = best;
                secondScore = bestScore;

                best = b;
                bestScore = score;
            }
            else if (score < secondScore)
            {
                second = b;
                secondScore = score;
            }
        }

        if (best == null && biomes != null && biomes.Length > 0)
        {
            best = biomes[0];
            bestScore = 0f;
        }
    }

    // --- BORDER WALLS + ENTRANCES ---

    List<Vector3Int> entrances = new List<Vector3Int>();

    void GenerateBorderWallsWithEntrances()
    {
        entrances.Clear();

        // Choose entrances on the bottom border (y=1) so stairs go "up" into map.
        // (You can extend this to other sides later.)
        for (int i = 0; i < entranceCount; i++)
        {
            int ex = Random.Range(2, width - 2 - entranceWidth);
            entrances.Add(new Vector3Int(ex, 1, 0));
        }

        // build border walls on y=0 and y=height-1, x=0 and x=width-1
        for (int x = 0; x < width; x++)
        {
            PlaceWallCell(new Vector3Int(x, 0, 0));
            PlaceWallCell(new Vector3Int(x, height - 1, 0));
        }
        for (int y = 0; y < height; y++)
        {
            PlaceWallCell(new Vector3Int(0, y, 0));
            PlaceWallCell(new Vector3Int(width - 1, y, 0));
        }

        // add an inner border too (x=1 / y=1) so the outside ocean + border feels separated
        for (int x = 1; x < width - 1; x++)
            PlaceWallCell(new Vector3Int(x, 1, 0));

        // carve entrances in the inner bottom border line at y=1
        foreach (var e in entrances)
        {
            for (int w = 0; w < entranceWidth; w++)
            {
                Vector3Int p = new Vector3Int(e.x + w, e.y, 0);
                wallTilemap.SetTile(p, null);
                wallCells.Remove(p);
            }
        }
    }

    void PlaceWallCell(Vector3Int pos)
    {
        // never place walls on the very inside water if you don’t want — but borders should always be walls
        wallTilemap.SetTile(pos, wallPreset.single);
        wallCells.Add(pos);
    }

    bool IsWall(Vector3Int p) => wallTilemap.GetTile(p) != null;

    void ResolveAllWalls()
    {
        foreach (var pos in wallCells)
            ResolveWallTile(pos);
    }

    void ResolveWallTile(Vector3Int pos)
    {
        bool up = IsWall(pos + Vector3Int.up);
        bool down = IsWall(pos + Vector3Int.down);
        bool left = IsWall(pos + Vector3Int.left);
        bool right = IsWall(pos + Vector3Int.right);

        int connections = (up ? 1 : 0) + (down ? 1 : 0) + (left ? 1 : 0) + (right ? 1 : 0);
        TileBase tile = wallPreset.single;

        if (connections == 2)
        {
            if (up && down) tile = wallPreset.left;               // vertical run
            else if (left && right) tile = wallPreset.top;        // horizontal run
            else if (up && right) tile = wallPreset.bl;
            else if (up && left) tile = wallPreset.br;
            else if (down && right) tile = wallPreset.tl;
            else if (down && left) tile = wallPreset.tr;
        }
        else if (connections == 3)
        {
            if (!up) tile = wallPreset.top;
            else if (!down) tile = wallPreset.bottom;
            else if (!left) tile = wallPreset.left;
            else tile = wallPreset.right;
        }
        else if (connections == 1)
        {
            if (up) tile = wallPreset.bottom;
            else if (down) tile = wallPreset.top;
            else if (left) tile = wallPreset.right;
            else tile = wallPreset.left;
        }
        else
        {
            tile = wallPreset.single;
        }

        wallTilemap.SetTile(pos, tile);
    }

    // --- PATHS (landmarks -> paths) ---

    void GeneratePaths()
    {
        pathCells.Clear();

        List<Vector2Int> landmarks = GenerateLandmarks(landmarkCount);
        if (landmarks.Count < 2) return;

        for (int i = 0; i < landmarks.Count - 1; i++)
            GeneratePath(landmarks[i], landmarks[i + 1]);
    }

    List<Vector2Int> GenerateLandmarks(int count)
    {
        List<Vector2Int> points = new List<Vector2Int>();
        int attempts = count * 30;

        while (points.Count < count && attempts-- > 0)
        {
            int x = Random.Range(2, width - 2);
            int y = Random.Range(2, height - 2);

            Vector3Int pos = new Vector3Int(x, y, 0);

            // avoid water/sand and avoid walls
            if (groundTilemap.GetTile(pos) == waterTile) continue;
            if (groundTilemap.GetTile(pos) == sandTile) continue;
            if (wallTilemap.GetTile(pos) != null) continue;

            points.Add(new Vector2Int(x, y));
        }

        if (points.Count == 0)
            points.Add(new Vector2Int(width / 2, height / 2));

        return points;
    }

    void GeneratePath(Vector2Int start, Vector2Int end)
    {
        Vector2Int current = start;
        int safety = width * height * 2;

        while (current != end && safety-- > 0)
        {
            Vector3Int cell = new Vector3Int(current.x, current.y, 0);

            if (wallTilemap.GetTile(cell) == null &&
                groundTilemap.GetTile(cell) != waterTile &&
                groundTilemap.GetTile(cell) != sandTile)
            {
                pathCells.Add(cell);
            }

            if (Random.value > 0.5f)
                current.x += Mathf.Clamp(end.x - current.x, -1, 1);
            else
                current.y += Mathf.Clamp(end.y - current.y, -1, 1);
        }

        Vector3Int endCell = new Vector3Int(end.x, end.y, 0);
        if (wallTilemap.GetTile(endCell) == null)
            pathCells.Add(endCell);
    }

    void ResolveAllPaths()
    {
        foreach (Vector3Int pos in pathCells)
            ResolvePathTile(pos);
    }

    bool IsPath(Vector3Int pos) => pathCells.Contains(pos);

    void ResolvePathTile(Vector3Int pos)
    {
        if (wallTilemap.GetTile(pos) != null) return;

        bool up = IsPath(pos + Vector3Int.up);
        bool down = IsPath(pos + Vector3Int.down);
        bool left = IsPath(pos + Vector3Int.left);
        bool right = IsPath(pos + Vector3Int.right);

        int connections = (up ? 1 : 0) + (down ? 1 : 0) + (left ? 1 : 0) + (right ? 1 : 0);

        TileBase tile = pathPreset.straightHorizontal;

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
        else if (connections == 1)
        {
            if (up || down) tile = pathPreset.straightVertical;
            else tile = pathPreset.straightHorizontal;
        }

        pathTilemap.SetTile(pos, tile);
    }

    // --- STAIRS at ENTRANCES (multi-tile) ---
    void PlaceEntranceStairs()
    {
        if (stairPresets == null) return;

        foreach (var e in entrances)
        {
            // stairs go upward into the map from the opening at y=1
            // place at y=2,3,4 (bottom/middle/top) so it doesn't overwrite the carved wall line.
            int x = e.x;
            int y = e.y + 1;

            bool useRight = (x < width / 2); // simple rule: left side uses right-up stairs, right side uses left-up stairs
            PlaceStairColumn(new Vector3Int(x, y, 0), useRight);
        }
    }

    void PlaceStairColumn(Vector3Int bottomPos, bool upRight)
    {
        // Height assumed 3 (bottom/middle/top) based on your sprite structure.
        // If you change height, extend this loop + add more arrays.
        TileBase b = upRight ? stairPresets.Pick(stairPresets.bottomRight) : stairPresets.Pick(stairPresets.bottomLeft);
        TileBase m = upRight ? stairPresets.Pick(stairPresets.middleRight) : stairPresets.Pick(stairPresets.middleLeft);
        TileBase t = upRight ? stairPresets.Pick(stairPresets.topRight) : stairPresets.Pick(stairPresets.topLeft);

        if (b != null) pathTilemap.SetTile(bottomPos, b);
        if (m != null) pathTilemap.SetTile(bottomPos + Vector3Int.up, m);
        if (t != null) pathTilemap.SetTile(bottomPos + Vector3Int.up * 2, t);
    }

    // --- PROPS ---
    bool IsWaterOrSand(Vector3Int pos)
    {
        var t = groundTilemap.GetTile(pos);
        return t == waterTile || t == sandTile;
    }

    bool CellOccupied(Vector3Int pos)
    {
        return (propBackTilemap.GetTile(pos) != null) || (propFrontTilemap.GetTile(pos) != null);
    }

    void GenerateProps()
    {
        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 2; y < height - 1; y++) // start at 2 to avoid entrance line
            {
                Vector3Int pos = new Vector3Int(x, y, 0);

                if (IsWaterOrSand(pos)) continue;
                if (wallTilemap.GetTile(pos) != null) continue;
                if (pathTilemap.GetTile(pos) != null) continue;
                if (CellOccupied(pos)) continue;

                foreach (PropPreset prop in props)
                {
                    if (prop == null) continue;
                    if (!prop.Matches(heightMap[x, y], moistureMap[x, y], heatMap[x, y])) continue;
                    if (Random.value >= prop.spawnChance) continue;

                    // extra guard: avoid stacking from multiple passes
                    if (extraPropClearChance >= 1f || Random.value < extraPropClearChance)
                    {
                        if (CellOccupied(pos)) break;
                    }

                    var tm = prop.isTall ? propFrontTilemap : propBackTilemap;
                    tm.SetTile(pos, prop.GetRandomTile());
                    break; // one prop max per tile
                }
            }
        }
    }

    // --- PLAYER SPAWN ---
    bool CanSpawnAt(Vector3Int pos)
    {
        // keep player inside border
        if (pos.x <= 1 || pos.y <= 1 || pos.x >= width - 2 || pos.y >= height - 2)
            return false;

        if (IsWaterOrSand(pos)) return false;
        if (wallTilemap.GetTile(pos) != null) return false;
        if (pathTilemap.GetTile(pos) != null) return false;
        if (CellOccupied(pos)) return false;

        return true;
    }

    void SpawnPlayerRandom()
    {
        if (player == null) return;

        for (int i = 0; i < spawnAttempts; i++)
        {
            int x = Random.Range(2, width - 2);
            int y = Random.Range(2, height - 2);

            Vector3Int cell = new Vector3Int(x, y, 0);
            if (!CanSpawnAt(cell)) continue;

            player.position = groundTilemap.GetCellCenterWorld(cell);
            return;
        }

        player.position = groundTilemap.GetCellCenterWorld(new Vector3Int(width / 2, height / 2, 0));
    }
}
