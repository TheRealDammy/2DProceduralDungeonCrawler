using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RandomDungeonGenerator : AbstractDungeonGenerator
{
    [SerializeField] protected SimpleRandomDungeonData[] RandomDungeonData;
    [SerializeField] protected DungeonData dungeonData;

    protected override void RunProceduralGeneration()
    {
        HashSet<Vector2Int> floorPositions = RunRandomWalks(RandomDungeonData, startPosition);
        tileMapGenerator.Clear();      
        tileMapGenerator.PaintFloorTiles(floorPositions);
        WallGenerator.CreateWalls(floorPositions, tileMapGenerator);
    }

    protected HashSet<Vector2Int> RunRandomWalks(SimpleRandomDungeonData[] data, Vector2Int position)
    {
        var currentPosition = position;
        HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();
        for (int i = 0; i < RandomDungeonData[UnityEngine.Random.Range(0, RandomDungeonData.Length)].iterations; i++)
        {
            var path = ProceduralGenerationAlgorithms.RandomWalk(currentPosition, RandomDungeonData[UnityEngine.Random.Range(0, RandomDungeonData.Length)].walkLength);
            floorPositions.UnionWith(path);
            if (RandomDungeonData[UnityEngine.Random.Range(0, RandomDungeonData.Length)].startRandomly)
            {
                currentPosition = floorPositions.ElementAt(UnityEngine.Random.Range(0, floorPositions.Count));
            }
        }

        return floorPositions;
    }
}
