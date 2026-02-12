using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(EnemyHealth))]
public class EnemyController : MonoBehaviour
{
    [Header("Runtime")]
    [SerializeField] private Transform target;
    [SerializeField] private LayerMask hitLayers;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private bool isInitialized;  


    private Rigidbody2D rb;
    private EnemyHealth health;
    private SpriteRenderer sr;
    private Animator animator;

    private EnemyTypeSO type;
    private EnemyVariant variant;
    public EnemyVariantData variantData
    {
        get
        {
            if (type == null) return null;
            int vi = (int)variant;
            if (vi < 0 || vi >= type.variants.Length) return null;
            return type.variants[vi];
        }
    }

    private enum State { Patrol, Chase, Attack, Dead }
    private State currentState;

    private Vector2 patrolTarget;
    private Vector2 facingDir = Vector2.down;

    private float lastAttackTime = -999f;

    private float moveSpeed;
    private float aggroRange;
    private float attackRange;
    private float attackCooldown;
    private int damage;

    private bool isAttacking;

    #region UNITY

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<EnemyHealth>();
        sr = GetComponentInChildren<SpriteRenderer>(true);
        animator = GetComponentInChildren<Animator>(true);

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        rb.mass = 100f;
        rb.linearDamping = 8f;
        rb.angularDamping = 999f;

        currentState = State.Patrol;
        SetNewPatrolTarget();
    }

    private void Update()
    {
        if (!isInitialized) return;

        if (animator != null)
        {
            animator.SetFloat("Horizontal", facingDir.x);
            animator.SetFloat("Vertical", facingDir.y);
        }
    }

    private void FixedUpdate()
    {
        if (!isInitialized || target == null || type == null)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (health.isDead)
        {
            currentState = State.Dead;
        }

        switch (currentState)
        {
            case State.Patrol:
                UpdatePatrol();
                break;

            case State.Chase:
                UpdateChase();
                break;

            case State.Attack:
                UpdateAttack();
                break;

            case State.Dead:
                rb.linearVelocity = Vector2.zero;
                break;
        }
    }

    #endregion

    #region INITIALIZATION

    public bool Init(EnemyTypeSO enemyType, EnemyVariant enemyVariant, Transform player)
    {
        if (enemyType == null || player == null || health == null)
            return false;

        int vi = (int)enemyVariant;
        if (enemyType.variants == null || vi < 0 || vi >= enemyType.variants.Length)
            return false;

        var v = enemyType.variants[vi];
        if (v == null) return false;

        type = enemyType;
        variant = enemyVariant;
        target = player;

        moveSpeed = Mathf.Max(0.01f, type.moveSpeed * v.speedMultiplier);
        aggroRange = Mathf.Max(0f, type.aggroRange);
        attackRange = Mathf.Max(0.05f, type.attackRange);
        attackCooldown = Mathf.Max(0.05f, type.attackCooldown);
        damage = Mathf.Max(1, Mathf.RoundToInt(type.baseDamage * v.damageMultiplier));

        int maxHP = Mathf.Max(1, Mathf.RoundToInt(type.baseHP * v.hpMultiplier));
        health.Init(maxHP);

        if (sr != null)
            sr.sprite = v.spriteOverride != null ? v.spriteOverride : type.defaultSprite;

        if (animator != null && v.animatorOverride != null)
            animator.runtimeAnimatorController = v.animatorOverride;

        float s = Mathf.Max(0.1f, v.scaleMultiplier);
        transform.localScale = new Vector3(s, s, 1f);

        isInitialized = true;
        rb.linearVelocity = Vector2.zero;
        rb.WakeUp();

        return true;
    }

    #endregion

    #region PATROL

    private void UpdatePatrol()
    {
        float distToPlayer = Vector2.Distance(transform.position, target.position);

        if (distToPlayer <= aggroRange && CanSeePlayer())
        {
            currentState = State.Chase;
            return;
        }

        MoveTowards(patrolTarget);

        if (Vector2.Distance(transform.position, patrolTarget) < 0.2f)
        {
            SetNewPatrolTarget();
        }
    }

    private void SetNewPatrolTarget()
    {
        patrolTarget = (Vector2)transform.position + Random.insideUnitCircle * 3f;
    }

    #endregion

    #region CHASE

    private void UpdateChase()
    {
        Vector2 toPlayer = target.position - transform.position;
        float dist = toPlayer.magnitude;

        if (dist > aggroRange * 1.2f)
        {
            currentState = State.Patrol;
            SetNewPatrolTarget();
            return;
        }

        float stopDistance = attackRange * 0.9f;

        if (dist <= stopDistance)
        {
            currentState = State.Attack;
            rb.linearVelocity = Vector2.zero;
            return;
        }

        facingDir = toPlayer.normalized;

        // Obstacle avoidance
        RaycastHit2D hit = Physics2D.Raycast(transform.position, facingDir, 0.6f, obstacleMask);

        if (hit.collider != null)
        {
            Vector2 side = new Vector2(-facingDir.y, facingDir.x);

            if (!Physics2D.Raycast(transform.position, side, 0.6f, obstacleMask))
                facingDir = side;
            else
                facingDir = -side;
        }

        // Separation
        Vector2 separation = Vector2.zero;
        Collider2D[] nearby = Physics2D.OverlapCircleAll(transform.position, 0.8f);

        foreach (var col in nearby)
        {
            if (col.gameObject == gameObject) continue;
            if (!col.CompareTag("Enemy")) continue;

            Vector2 away = (Vector2)(transform.position - col.transform.position);
            separation += away.normalized;
        }

        Vector2 finalDir = (facingDir + separation * 0.5f).normalized;

        Vector2 step = finalDir * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + step);

        if (animator != null)
            animator.SetFloat("Speed", 1f);
    }

    #endregion

    #region ATTACK

    private void UpdateAttack()
    {
        rb.linearVelocity = Vector2.zero;

        Vector2 toPlayer = target.position - transform.position;
        float dist = toPlayer.magnitude;

        if (dist > attackRange + 0.2f)
        {
            currentState = State.Chase;
            return;
        }

        facingDir = toPlayer.normalized;

        // Small strafe
        Vector2 strafe = new Vector2(-facingDir.y, facingDir.x);
        rb.MovePosition(rb.position + strafe * 0.5f * Time.fixedDeltaTime);

        TryAttack();
    }

    private void TryAttack()
    {
        if (isAttacking) return;
        if (Time.time < lastAttackTime + attackCooldown) return;

        StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        if (animator != null)
            animator.SetTrigger("Attack");

        yield return new WaitForSeconds(0.15f);

        // Small lunge
        rb.MovePosition(rb.position + facingDir * 0.2f);

        Vector2 center = (Vector2)transform.position + facingDir * 0.6f;
        float radius = 0.45f;

        Collider2D hit = Physics2D.OverlapCircle(center, radius, hitLayers);

        if (hit != null)
        {
            var dmg = hit.GetComponentInParent<IDamageable>();
            if (dmg != null)
                dmg.TakeDamage(damage, hit.ClosestPoint(center), facingDir);
        }

        yield return new WaitForSeconds(0.3f);

        isAttacking = false;
    }

    #endregion

    #region HELPERS

    private void MoveTowards(Vector2 targetPos)
    {
        Vector2 dir = (targetPos - rb.position).normalized;
        facingDir = dir;

        Vector2 step = dir * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + step);

        if (animator != null)
            animator.SetFloat("Speed", 1f);
    }

    private bool CanSeePlayer()
    {
        Vector2 dir = (target.position - transform.position).normalized;
        float dist = Vector2.Distance(transform.position, target.position);

        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, dist, obstacleMask);

        return hit.collider == null;
    }

    #endregion
}
