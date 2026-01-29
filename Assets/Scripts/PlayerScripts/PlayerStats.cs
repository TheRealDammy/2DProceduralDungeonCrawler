using System.Collections.Generic;
using UnityEngine;

public enum PlayerStatType
{
    Health,
    Stamina,
    Strength,
    Durability
}

[System.Serializable]
public class StatData
{
    public int level;
    public int hardCap;
    public int softCap;
}

public class PlayerStats : MonoBehaviour
{
    private Dictionary<PlayerStatType, StatData> stats;

    private void Awake()
    {
        stats = new Dictionary<PlayerStatType, StatData>()
        {
            { PlayerStatType.Health,     new StatData { level = 1, softCap = 10, hardCap = 20 } },
            { PlayerStatType.Stamina,    new StatData { level = 1, softCap = 10, hardCap = 20 } },
            { PlayerStatType.Strength,   new StatData { level = 1, softCap = 8,  hardCap = 15 } },
            { PlayerStatType.Durability, new StatData { level = 1, softCap = 8,  hardCap = 15 } },
        };
    }

    public int GetStatLevel(PlayerStatType type)
    {
        return stats[type].level;
    }

    public bool CanUpgrade(PlayerStatType type)
    {
        return stats[type].level < stats[type].hardCap;
    }

    public bool UpgradeStat(PlayerStatType type)
    {
        if (!CanUpgrade(type)) return false;

        stats[type].level++;
        return true;
    }

    public bool RefundStat(PlayerStatType type)
    {
        if (stats[type].level <= 0) return false;

        stats[type].level--;
        return true;
    }

    public float GetEffectMultiplier(PlayerStatType type)
    {
        var stat = stats[type];

        if (stat.level <= stat.softCap)
            return stat.level;

        float reduced = stat.softCap +
                         (stat.level - stat.softCap) * 0.5f;

        return reduced;
    }

    public StatData GetStatData(PlayerStatType type)
    {
        return stats[type];
    }
}
