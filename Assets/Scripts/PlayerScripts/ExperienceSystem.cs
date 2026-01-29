using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExperienceSystem : MonoBehaviour
{
    [SerializeField] private int level = 1;
    [SerializeField] private int currentXP;
    [SerializeField] private int xpToNext = 100;
    [SerializeField] private int xpGrowth = 50;
    [SerializeField] private int statPointsPerLevel = 5;

    private int currentStatPoints;

    [Header("UI")]
    [SerializeField] private Image xpBar;
    [SerializeField] private TextMeshProUGUI levelText;

    private void Start()
    {
        UpdateUI();
    }

    public void AddXP(int amount)
    {
        currentXP += amount;

        while (currentXP >= xpToNext)
        {
            currentXP -= xpToNext;
            LevelUp();
        }

        UpdateUI();
    }

    private void LevelUp()
    {
        level++;
        currentStatPoints += statPointsPerLevel;
        xpToNext += xpGrowth;
        Debug.Log($"Leveled up to {level}! You have {currentStatPoints} stat points to spend.");
    }

    public bool SpendStatPoint()
    {
        if (currentStatPoints <= 0) return false;
        currentStatPoints--;
        return true;
    }

    public int GetStatPoints()
    {
        return currentStatPoints;
    }

    public void RefundStatPoint()
    {
        currentStatPoints++;
    }

    public void SetStatPoints(int amount)
    {
        currentStatPoints = amount;
    }

    private void UpdateUI()
    {
        xpBar.fillAmount = (float)currentXP / xpToNext;
        levelText.text = $"Level {level}";
    }
}
