using UnityEngine;

public class Enemy : MonoBehaviour, IDamagable
{
    [Header("Stat")]
    [SerializeField] private EnemyData data;
    private int currentHp;
    private bool isDead;
    public bool IsDead => isDead;
    
    [Header("AI Behavior")]
    private float lastAttackTime;
    
    [Header("Animation")]
    [SerializeField] private AnimatorOverrideController overrideController;
    [SerializeField] private EnemyAnimationHandler animHandler;
    
    [Header("Components")]
    private Animator animator;
    private Collider enemyCollider;
    
    private void Awake()
    {
        animHandler = GetComponent<EnemyAnimationHandler>();
        enemyCollider = GetComponent<Collider>();
        
        if (animHandler == null) Debug.LogError("animHandler is null");
        if (enemyCollider == null ) Debug.LogError("enemyCollider is null");
        
        animHandler.OnDieEndCallback -= OnDie;
        animHandler.OnDieEndCallback += OnDie;
    }

    
    private void Start()
    {
        animHandler.SetOverrideController(data.overrideController);
        
        currentHp = data.maxHp;
        animHandler.PlayIdle();
    }
    
    
    private void Update()
    {
        if (isDead) return;
    
        Player target = CharacterManager.Player;
        if (target == null) return;
    
        float distanceToPlayer = Vector3.Distance(transform.position, target.transform.position);
    
        // 공격 범위 내에 있으면 공격
        if (distanceToPlayer <= data.attackRange)
        {
            AttackPlayer(target);
        }
        else
        {
            // 플레이어 추적
            ChasePlayer(target);
        }
    }


    public void TakeDamage(int damage)
    {
        if (isDead) return;
        
        currentHp -= damage;
        animHandler.PlayHit();
        
        if (currentHp <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        if (isDead) return;
        
        isDead = true;
        
        if (enemyCollider != null)
        {
            enemyCollider.enabled = false;
        }
        
        animHandler.PlayDie();
    }
    
    private void OnDie()
    {
        Destroy(gameObject);
    }
    
    private void ChasePlayer(Player target)
    {
        // 플레이어 방향 계산
        Vector3 direction = (target.transform.position - transform.position).normalized;
    
        // 이동
        transform.position += direction * (data.moveSpeed * Time.deltaTime);
    
        // 플레이어 바라보기
        Vector3 lookDirection = new Vector3(direction.x, 0, direction.z);
        if (lookDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(lookDirection);
        }
    
        // 이동 애니메이션
        animHandler.PlayMove();
    }
    
    
    private void AttackPlayer(Player target)
    {
        // 쿨다운 체크
        if (Time.time - lastAttackTime < data.attackCooldown) return;
    
        lastAttackTime = Time.time;
    
        // 플레이어에게 데미지
        int damage = data.attackPower;
        target.StatHandler.ModifyStat(StatType.Hp, -damage);
    
        // 공격 애니메이션
        animHandler.PlayAttack();
    
        Debug.Log($"{name}이 플레이어에게 {damage} 데미지!");
    }

    
    // public getters
    public EnemyType GetEnemyType => data.type;
}
