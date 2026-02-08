using UnityEngine;

public class MageCombatController : CombatController
{
    [SerializeField] GameObject spellPrefab;
    [SerializeField] private Transform castPoint;
    [SerializeField] float radius = 0.2f;
    [SerializeField] private LayerMask hitMask;

    protected override void ExecuteAttack()
    {
        lastAttackTime = Time.time;

        animator.SetTrigger("Cast");

        Vector2 center = castPoint.position;
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius, hitMask);

        int dmg = GetFinalDamage();

        foreach (var hit in hits)
        {
            IDamageable d = hit.GetComponentInParent<IDamageable>();
            if (d != null)
                d.TakeDamage(dmg, center, Vector2.zero);
        }

        HitStop();
        CameraShake.Instance.Shake(0.25f, 0.12f);
    }
}
