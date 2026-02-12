using UnityEngine;

public class DamageNumberSpawner : MonoBehaviour
{
    public static DamageNumberSpawner Instance;

    [SerializeField] private GameObject damageNumberPrefab;
    public static bool enabledGlobal = true;

    private void Awake()
    {
        Instance = this;
    }

    public void Spawn(int amount, Vector2 position, bool crit = false)
    {
        if (!enabledGlobal) return;

        Debug.Log($"Spawning damage number: {amount} at {position} (Crit: {crit})");

        GameObject obj =
            Instantiate(damageNumberPrefab, position, Quaternion.identity);

        var number = obj.GetComponent<DamageNumber>();

        number.Init(amount, crit ? Color.yellow : Color.white);
    }
}
