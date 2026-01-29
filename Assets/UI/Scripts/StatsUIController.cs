using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatsUIController : MonoBehaviour
{
    [Header("Core")]
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private ExperienceSystem expSystem;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private TopDownCharacterController controller;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI pointsText;

    [Header("Health")]
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private Image healthBar;

    [Header("Stamina")]
    [SerializeField] private TextMeshProUGUI staminaText;
    [SerializeField] private Image staminaBar;

    [Header("Strength")]
    [SerializeField] private TextMeshProUGUI strengthText;
    [SerializeField] private Image strengthBar;

    [Header("Durability")]
    [SerializeField] private TextMeshProUGUI durabilityText;
    [SerializeField] private Image durabilityBar;

    private void Awake()
    {
        RefreshUI();
    }

    // =============================
    // UPGRADE / REFUND
    // =============================
    public void Upgrade(PlayerStatType type)
    {
        if (!expSystem.SpendStatPoint()) return;
        if (!playerStats.UpgradeStat(type)) return;

        ApplyStats();
        RefreshUI();
    }

    public void Refund(PlayerStatType type)
    {
        if (!playerStats.RefundStat(type)) return;

        expSystem.RefundStatPoint();
        ApplyStats();
        RefreshUI();
    }

    // =========================
    // BUTTON CALLBACKS
    // =========================
    public void UpgradeHealth()
    {
        Upgrade(PlayerStatType.Health);
    }

    public void UpgradeStamina()
    {
        Upgrade(PlayerStatType.Stamina);
    }

    public void UpgradeStrength()
    {
        Upgrade(PlayerStatType.Strength);
    }

    public void UpgradeDurability()
    {
        Upgrade(PlayerStatType.Durability);
    }

    public void RefundHealth()
    {
        Refund(PlayerStatType.Health);
    }

    public void RefundStamina()
    {
        Refund(PlayerStatType.Stamina);
    }

    public void RefundStrength()
    {
        Refund(PlayerStatType.Strength);
    }

    public void RefundDurability()
    {
        Refund(PlayerStatType.Durability);
    }

    // =============================
    private void ApplyStats()
    {
        playerHealth.ApplyStats();
        controller.ApplyStats();
    }

    // =============================
    private void RefreshUI()
    {
        UpdateRow(PlayerStatType.Health, healthText, healthBar);
        UpdateRow(PlayerStatType.Stamina, staminaText, staminaBar);
        UpdateRow(PlayerStatType.Strength, strengthText, strengthBar);
        UpdateRow(PlayerStatType.Durability, durabilityText, durabilityBar);

        pointsText.text = $"Stat Points: {expSystem.GetStatPoints()}";
    }

    private void UpdateRow(PlayerStatType type, TextMeshProUGUI text, Image bar)
    {
        var data = playerStats.GetStatData(type);
        text.text = $"{type}: {data.level}/{data.hardCap}";
        bar.fillAmount = (float)data.level / data.hardCap;
    }
}
