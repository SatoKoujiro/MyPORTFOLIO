using UnityEngine;

public class UnitActor : MonoBehaviour
{
    private ActorVisual visual;
    private float attackCooldown;

    public string DisplayName { get; private set; }
    public OperatorRole Role { get; private set; }
    public Vector2Int GridPosition { get; private set; }
    public int Lane => GridPosition.y;
    public int CurrentHealth { get; private set; }
    public int MaxHealth { get; private set; }
    public int AttackDamage { get; private set; }
    public float AttackInterval { get; private set; }
    public float AttackRange { get; private set; }
    public int BlockCapacity { get; private set; }
    public int HealingAmount { get; private set; }
    public bool IsAlive => CurrentHealth > 0;
    public bool IsAttackReady => attackCooldown <= 0f;

    private void Awake()
    {
        visual = GetComponent<ActorVisual>();
    }

    public void Initialize(GameConfig.OperatorDefinition definition, Vector2Int gridPosition)
    {
        ActorHealthBar.Attach(gameObject);
        DisplayName = definition.displayName;
        Role = definition.role;
        GridPosition = gridPosition;
        MaxHealth = definition.maxHealth;
        CurrentHealth = MaxHealth;
        AttackDamage = definition.attackDamage;
        AttackInterval = definition.attackInterval;
        AttackRange = definition.attackRange;
        BlockCapacity = definition.blockCapacity;
        HealingAmount = definition.healingAmount;
        attackCooldown = 0.2f;

        if (visual != null)
        {
            visual.SetStyle(definition.color, 0.76f, 5);
        }
    }

    public void AdvanceAttackClock(float deltaTime)
    {
        attackCooldown -= deltaTime;
    }

    public void Attack(EnemyActor target, float attackInterval)
    {
        if (!IsAttackReady || target == null || !target.IsAlive)
        {
            return;
        }

        target.TakeDamage(AttackDamage);
        attackCooldown = attackInterval;
    }

    public void Heal(UnitActor target)
    {
        if (!IsAttackReady || target == null || !target.IsAlive || target.CurrentHealth >= target.MaxHealth)
        {
            return;
        }

        target.RestoreHealth(HealingAmount);
        attackCooldown = AttackInterval;
    }

    public void Relocate(Vector2Int gridPosition, Vector3 position)
    {
        GridPosition = gridPosition;
        transform.position = position;
    }

    public void TakeDamage(int damage)
    {
        CurrentHealth = Mathf.Max(0, CurrentHealth - damage);
    }

    public void RestoreHealth(int amount)
    {
        CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
    }
}