using UnityEngine;

public class ArcherCombatController : CombatController
{
    [SerializeField] GameObject arrowPrefab;
    [SerializeField] Transform firePoint;
    [SerializeField] float arrowSpeed = 12f;
    [SerializeField] LayerMask hitMask;
    private int damage;

    protected override void Awake()
    {
        base.Awake();
        if (firePoint == null)
        {
            firePoint = transform.Find("FirePoint");
        }
        if (firePoint == null)
        {
            Debug.LogError("FirePoint missing on ArcherCombat prefab");
        }
        if (hitMask == 0)
        {
            hitMask = LayerMask.GetMask("Enemies", "Props");
            Debug.LogWarning("HitMask auto-assigned");
        }

        baseDamage = damage;
    }

    protected override void ExecuteAttack()
    {
        lastAttackTime = Time.time;

        animator.SetTrigger("Attack");

        GameObject arrow = Instantiate(arrowPrefab, firePoint.position, Quaternion.identity);
        Rigidbody2D rb = arrow.GetComponent<Rigidbody2D>();

        rb.linearVelocity = lastDirection.normalized * arrowSpeed;

        ArrowProjectile proj = arrow.GetComponent<ArrowProjectile>();
        proj.SetDamage(GetFinalDamage(), owner);


        HitStop();
        CameraShake.Instance.Shake(0.25f, 0.12f);
    }
}