using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Base Health")]
    [SerializeField] private float baseHealth = 100f;

    [Header("Damage Handling")]
    [SerializeField] private float iFrames = 0.5f;
    [SerializeField] private float flashTime = 0.06f;

    [Header("UI")]
    [SerializeField] private Image healthBar;

    private float maxHealth;
    private float currentHealth;
    private bool invincible;

    private SpriteRenderer sr;
    private Color originalColor;

    private PlayerStats stats;
    private ExperienceSystem expSystem;

    // =========================
    // INITIALIZATION
    // =========================
    private void Awake()
    {
        stats = GetComponent<PlayerStats>();
        expSystem = GetComponent<ExperienceSystem>();
        sr = GetComponent<SpriteRenderer>();
        maxHealth = baseHealth;
        currentHealth = maxHealth;

        if (sr)
            originalColor = sr.color;

        ApplyStats(true);
    }

    // =========================
    // STAT APPLICATION
    // =========================
    public void ApplyStats(bool fullHeal = false)
    {
        float oldMax = maxHealth;

        maxHealth = baseHealth + stats.GetStatLevel(PlayerStatType.Health) * 20f;

        if (fullHeal || oldMax <= 0)
            currentHealth = maxHealth;
        else
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHealthUI();
    }

    // =========================
    // DAMAGE
    // =========================
    public void TakeDamage(int amount, Vector2 hitPoint, Vector2 hitDirection)
    {
        if (invincible) return;

        int finalDamage = CalculateReducedDamage(amount);
        currentHealth -= finalDamage;

        UpdateHealthUI();

        if (sr)
            StartCoroutine(Flash());

        StartCoroutine(IFrames());    

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private int CalculateReducedDamage(int incoming)
    {
        float reductionPercent = stats.GetStatLevel(PlayerStatType.Durability) * 0.04f;
        reductionPercent = Mathf.Clamp01(reductionPercent);

        return Mathf.Max(1, Mathf.RoundToInt(incoming * (1f - reductionPercent)));
    }

    // =========================
    // HEALING
    // =========================
    public void Heal(int amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        UpdateHealthUI();
    }

    // =========================
    // FEEDBACK
    // =========================
    private IEnumerator IFrames()
    {
        invincible = true;
        yield return new WaitForSeconds(iFrames);
        invincible = false;
    }

    private IEnumerator Flash()
    {
        sr.color = Color.red;
        yield return new WaitForSeconds(flashTime);
        sr.color = originalColor;
    }

    // =========================
    // UI
    // =========================
    private void UpdateHealthUI()
    {
        healthBar.fillAmount = currentHealth / maxHealth;
        Debug.Log($"Health: {currentHealth}/{maxHealth}");
    }

    // =========================
    // DEATH
    // =========================
    private void Die()
    {
        SceneManager.LoadSceneAsync("DeathScene");
        enabled = false;
    }
}
