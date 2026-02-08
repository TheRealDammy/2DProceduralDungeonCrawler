using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterSelectController : MonoBehaviour
{
    public static CharacterSelectController Instance { get; private set; }

    [Header("Class Data")]
    [SerializeField] private CharacterClassData swordsman;
    [SerializeField] private CharacterClassData archer;
    [SerializeField] private CharacterClassData mage;

    public CharacterClassData selectedClassData;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SelectSwordsman() => Select(swordsman);
    public void SelectArcher() => Select(swordsman);
    public void SelectMage() => Select(swordsman);

    public void SelectRandom()
    {
        CharacterClassData[] all = { swordsman };
        Select(all[Random.Range(0, all.Length)]);
    }

    private void Select(CharacterClassData data)
    {
        selectedClassData = data;
        Debug.Log($"Selected class data: {data.characterClass}");
        SceneManager.LoadScene("Dungeon");
    }
}

