using UnityEngine;

public class EnemyActor : MonoBehaviour
{
    private ActorVisual visual;
    private float attackCooldown;
    private Vector3[] route;
    private int nextWaypointIndex;

    public string DisplayName { get; private set; }
    public int Lane { get; private set; }
    public int CurrentHealth { get; private set; }
    public int MaxHealth { get; private set; }
    public int AttackDamage { get; private set; }
    public float AttackInterval { get; private set; }
    public float MoveSpeed { get; private set; }
    public bool IsAlive => CurrentHealth > 0;
    public bool IsAttackReady => attackCooldown <= 0f;
    public bool HasReachedGoal { get; private set; }

    private void Awake()
    {
        visual = GetComponent<ActorVisual>();
    }

    public void Initialize(GameConfig.EnemyDefinition definition, int lane, Vector3[] enemyRoute)
    {
        ActorHealthBar.Attach(gameObject);
        DisplayName = definition.displayName;
        Lane = lane;
        MaxHealth = definition.maxHealth;
        CurrentHealth = definition.maxHealth;
        AttackDamage = definition.attackDamage;
        AttackInterval = definition.attackInterval;
        MoveSpeed = definition.moveSpeed;
        attackCooldown = 0.4f;
        route = enemyRoute;
        nextWaypointIndex = 1;
        HasReachedGoal = route == null || route.Length < 2;

        if (visual != null)
        {
            visual.SetStyle(definition.color, 0.46f, 6);
        }
    }

    public void AdvanceAttackClock(float deltaTime)
    {
        attackCooldown -= deltaTime;
    }

    public void Attack(UnitActor target)
    {
        if (!IsAttackReady || target == null || !target.IsAlive)
        {
            return;
        }

        target.TakeDamage(AttackDamage);
        attackCooldown = AttackInterval;
    }

    public void MoveToward(float targetX, float deltaTime)
    {
        Vector3 position = transform.position;
        position.x = Mathf.MoveTowards(position.x, targetX, MoveSpeed * deltaTime);
        transform.position = position;
    }

    public void MoveAlongRoute(float deltaTime)
    {
        if (HasReachedGoal || route == null || route.Length == 0)
        {
            HasReachedGoal = true;
            return;
        }

        Vector3 target = route[nextWaypointIndex];
        transform.position = Vector3.MoveTowards(transform.position, target, MoveSpeed * deltaTime);
        if (Vector3.Distance(transform.position, target) <= 0.001f)
        {
            nextWaypointIndex++;
            if (nextWaypointIndex >= route.Length)
            {
                HasReachedGoal = true;
            }
        }
    }

    public void TakeDamage(int damage)
    {
        CurrentHealth = Mathf.Max(0, CurrentHealth - damage);
    }
}