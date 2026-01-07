using UnityEngine;
using UnityEngine.Events;

public abstract class AbstractDungeonGenerator : MonoBehaviour
{
    [SerializeField] protected TileMapGenerator tileMapGenerator = null;
    [SerializeField] protected Vector2Int startPosition = Vector2Int.zero;

    public UnityEvent OnDungeonGenerated;

    public void GenerateDungeon()
    {
        tileMapGenerator.Clear();

        RunProceduralGeneration();

        OnDungeonGenerated?.Invoke();
    }

    protected abstract void RunProceduralGeneration();
}
