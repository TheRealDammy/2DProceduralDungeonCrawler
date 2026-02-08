using UnityEngine;

public class ArrowProjectile : MonoBehaviour
{
    private int damage;
    private Transform owner;

    public void SetDamage(int dmg, Transform ownerRef)
    {
        damage = dmg;
        owner = ownerRef;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.transform == owner) return;

        IDamageable dmg = col.GetComponentInParent<IDamageable>();
        if (dmg != null)
        {
            dmg.TakeDamage(damage, transform.position, transform.right);
        }

        Destroy(gameObject);
    }
}
