using System;
using System.Collections.Generic;
using System.Collections;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "BiomePreset", menuName = "New BiomePreset")]
public class BiomePreset : ScriptableObject
{
    public Sprite[] tiles;
    public float minHeight;
    public float minMoisture;
    public float minHeat;

    public Sprite GetTileSprite()
    {
        return tiles[UnityEngine.Random.Range(0, tiles.Length)];
    }

    public bool MatchConditions(float height, float moisture, float heat)
    {
        return height >= minHeight && moisture >= minMoisture && heat >= minHeat;
    }
}
