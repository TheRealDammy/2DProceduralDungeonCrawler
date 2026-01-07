using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomsFirstGenerator : RandomDungeonGenerator
{
    [SerializeField] private int minRoomWidth = 5;
    [SerializeField] private int minRoomHeight = 5;
    [SerializeField] private int dungeonWidth = 50;
    [SerializeField] private int dungeonHeight = 50;
    [SerializeField][Range(0, 10)] private int offset = 1;
    [SerializeField] private bool randomRoomPlacement = false;
    [SerializeField] private RoomDataExtractor roomDataExtractor;

    private void Start()
    {
        if (dungeonData != null)
            dungeonData.Reset();

        GenerateDungeon();
    }

    protected override void RunProceduralGeneration()
    {
        RoomsGeneration();
    }

    private void RoomsGeneration()
    {
        // Safety
        if (dungeonData != null)
        {
            Debug.Log($"GEN dungeonData: {dungeonData?.name}");
            dungeonData.Reset();
        }

        var roomsBounds = ProceduralGenerationAlgorithms.BinarySpacePartitioning(
            new BoundsInt((Vector3Int)startPosition, new Vector3Int(dungeonWidth, dungeonHeight, 0)),
            minRoomWidth + offset,
            minRoomHeight + offset
        );

        // Create rooms + global floor set
        HashSet<Vector2Int> floorPositions;
        List<Room> generatedRooms;

        if (randomRoomPlacement)
            (floorPositions, generatedRooms) = RandomCreateRoomsAndData(roomsBounds);
        else
            (floorPositions, generatedRooms) = CreateRoomsAndData(roomsBounds);

        // Connect rooms and mark corridors as "path"
        List<Vector2Int> roomCenters = generatedRooms
            .Select(r => Vector2Int.RoundToInt(r.RoomCenterPos))
            .ToList();

        HashSet<Vector2Int> corridors = ConnectRooms(roomCenters);
        floorPositions.UnionWith(corridors);

        // Store into DungeonData for extractor/props/enemies systems
        if (dungeonData != null)
        {
            dungeonData.rooms = generatedRooms;
            dungeonData.path = corridors; // treat corridors as path
        }

        // Paint tiles
        tileMapGenerator.PaintFloorTiles(floorPositions);
        WallGenerator.CreateWalls(floorPositions, tileMapGenerator);

        // Run extractor AFTER dungeon data is ready
        if (roomDataExtractor != null)
        {
            roomDataExtractor.ProcessRooms();
            Debug.Log($"EXTRACTOR dungeonData: {dungeonData?.name}");
        }


    }

    private (HashSet<Vector2Int> allFloors, List<Room> rooms) CreateRoomsAndData(List<BoundsInt> roomsBounds)
    {
        HashSet<Vector2Int> allFloors = new HashSet<Vector2Int>();
        List<Room> rooms = new List<Room>();

        foreach (var bounds in roomsBounds)
        {
            HashSet<Vector2Int> roomFloors = new HashSet<Vector2Int>();

            for (int col = offset; col < bounds.size.x - offset; col++)
            {
                for (int row = offset; row < bounds.size.y - offset; row++)
                {
                    Vector2Int pos = (Vector2Int)bounds.min + new Vector2Int(col, row);
                    roomFloors.Add(pos);
                }
            }

            allFloors.UnionWith(roomFloors);

            Vector2 center = new Vector2(bounds.center.x, bounds.center.y);
            rooms.Add(new Room(center, roomFloors));
        }

        return (allFloors, rooms);
    }

    private (HashSet<Vector2Int> allFloors, List<Room> rooms) RandomCreateRoomsAndData(List<BoundsInt> roomsBounds)
    {
        HashSet<Vector2Int> allFloors = new HashSet<Vector2Int>();
        List<Room> rooms = new List<Room>();

        for (int i = 0; i < roomsBounds.Count; i++)
        {
            var bounds = roomsBounds[i];
            var roomCenter = new Vector2Int(
                Mathf.RoundToInt(bounds.center.x),
                Mathf.RoundToInt(bounds.center.y)
            );

            // Random walk gives a big blob; clamp it inside the room bounds (with offset)
            var walked = RunRandomWalks(RandomDungeonData, roomCenter);

            HashSet<Vector2Int> roomFloors = new HashSet<Vector2Int>();
            foreach (var pos in walked)
            {
                if (pos.x >= (bounds.xMin + offset) && pos.x <= (bounds.xMax - offset) &&
                    pos.y >= (bounds.yMin + offset) && pos.y <= (bounds.yMax - offset))
                {
                    roomFloors.Add(pos);
                }
            }

            allFloors.UnionWith(roomFloors);

            Vector2 center = new Vector2(bounds.center.x, bounds.center.y);
            rooms.Add(new Room(center, roomFloors));
        }

        return (allFloors, rooms);
    }

    private HashSet<Vector2Int> ConnectRooms(List<Vector2Int> roomCenters)
    {
        HashSet<Vector2Int> corridors = new HashSet<Vector2Int>();
        if (roomCenters == null || roomCenters.Count == 0)
            return corridors;

        var currentRoomCenter = roomCenters[UnityEngine.Random.Range(0, roomCenters.Count)];
        roomCenters.Remove(currentRoomCenter);

        while (roomCenters.Count > 0)
        {
            Vector2Int closest = FindClosestRoomCenter(currentRoomCenter, roomCenters);
            roomCenters.Remove(closest);

            HashSet<Vector2Int> newCorridor = CreateCorridor(currentRoomCenter, closest);

            // Expand to 3x3 brush
            foreach (var pos in IncreaseCorridorBrush3by3(newCorridor.ToList()))
                corridors.Add(pos);

            currentRoomCenter = closest;
        }

        return corridors;
    }

    private HashSet<Vector2Int> CreateCorridor(Vector2Int from, Vector2Int to)
    {
        HashSet<Vector2Int> corridor = new HashSet<Vector2Int>();
        var pos = from;
        corridor.Add(pos);

        while (pos.y != to.y)
        {
            pos += (to.y > pos.y) ? Vector2Int.up : Vector2Int.down;
            corridor.Add(pos);
        }

        while (pos.x != to.x)
        {
            pos += (to.x > pos.x) ? Vector2Int.right : Vector2Int.left;
            corridor.Add(pos);
        }

        return corridor;
    }

    private List<Vector2Int> IncreaseCorridorBrush3by3(List<Vector2Int> corridor)
    {
        List<Vector2Int> expanded = new List<Vector2Int>();
        for (int i = 0; i < corridor.Count; i++)
        {
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    expanded.Add(corridor[i] + new Vector2Int(x, y));
                }
            }
        }
        return expanded;
    }

    private Vector2Int FindClosestRoomCenter(Vector2Int current, List<Vector2Int> centers)
    {
        Vector2Int closest = centers[0];
        float best = float.MaxValue;

        foreach (var c in centers)
        {
            float d = Vector2Int.Distance(current, c);
            if (d < best)
            {
                best = d;
                closest = c;
            }
        }

        return closest;
    }
}