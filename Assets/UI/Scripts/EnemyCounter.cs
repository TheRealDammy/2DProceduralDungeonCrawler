using TMPro;
using UnityEngine;

public class EnemyCounterUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI counterText;

    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnEnemyCountChanged += UpdateText;
            UpdateText(GameManager.Instance.AliveEnemies);
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnEnemyCountChanged -= UpdateText;
    }

    private void UpdateText(int count)
    {
        counterText.text = $"Enemies Remaining: {count}";
    }
}
