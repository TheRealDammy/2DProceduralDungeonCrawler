using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    [SerializeField] private Image healthBar;

    void Start()
    {
        currentHealth = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        healthBar.fillAmount = currentHealth / maxHealth;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        currentHealth -= Time.deltaTime * 2;
    }
}
