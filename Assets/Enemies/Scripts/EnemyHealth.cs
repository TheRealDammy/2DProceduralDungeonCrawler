using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    public int MaxHP { get; private set; }
    public int CurrentHP { get; private set; }

    [Header("Death")]
    [SerializeField] private float destroyDelay = 0.35f; // set to your death anim length

    private SpriteRenderer[] renderers;
    private Color[] originalColors;

    private Animator animator;
    private Rigidbody2D rb;
    private Collider2D[] colliders;

    private bool isDead;

    private void Awake()
    {
        renderers = GetComponentsInChildren<SpriteRenderer>(true);
        originalColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
            originalColors[i] = renderers[i] != null ? renderers[i].color : Color.white;

        animator = GetComponentInChildren<Animator>(true);
        rb = GetComponent<Rigidbody2D>();
        colliders = GetComponentsInChildren<Collider2D>(true);
    }

    public void Init(int maxHp)
    {
        MaxHP = Mathf.Max(1, maxHp);
        CurrentHP = MaxHP;
    }

    public void TakeDamage(int amount, Vector2 hitPoint, Vector2 hitDirection)
    {
        if (animator != null)
            animator.SetTrigger("Hurt");

        CurrentHP -= Mathf.Max(1, amount);

        if (CurrentHP <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (animator != null)
        {
            animator.SetBool("isDead", true);
        }

        // stop movement immediately
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        // disable colliders so it doesn't block / get hit multiple times
        if (colliders != null)
        {
            foreach (var c in colliders)
                if (c != null) c.enabled = false;
        }

        // destroy after delay so death anim can show
        Destroy(gameObject, Mathf.Max(0f, destroyDelay));
    }
}
