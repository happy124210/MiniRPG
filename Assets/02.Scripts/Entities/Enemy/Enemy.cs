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
        if (data != null)
        {
            InitializeWithData(data);
        }
    }
    
    /// <summary>
    /// 풀에서 가져올 때 호출되는 초기화 메서드
    /// </summary>
    public void InitializeFromPool(EnemyData enemyData)
    {
        InitializeWithData(enemyData);
        Debug.Log($"[Enemy] {enemyData.enemyName} 풀에서 초기화됨");
    }
    
    /// <summary>
    /// 초기화 로직
    /// </summary>
    private void InitializeWithData(EnemyData enemyData)
    {
        data = enemyData;
        
        // 애니메이션 override 설정
        if (animHandler != null && data.overrideController != null)
        {
            animHandler.SetOverrideController(data.overrideController);
        }
        
        // 스탯
        currentHp = data.maxHp;
        isDead = false;
        lastAttackTime = 0f;
        
        // 컴포넌트
        if (enemyCollider != null)
        {
            enemyCollider.enabled = true;
        }
        
        // 기본 애니메이션 상태로 설정
        if (animHandler != null)
        {
            animHandler.PlayIdle();
        }
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
        if (StageManager.Instance)
        {
            StageManager.Instance.OnEnemyKilled(this);
        }
    }
    
    /// <summary>
    /// 풀로 반환되기 전 정리 작업
    /// </summary>
    public void OnReturnToPool()
    {
        // 코루틴 정리
        StopAllCoroutines();
        
        // 상태 리셋
        isDead = false;
        currentHp = 0;
        lastAttackTime = 0f;
        
        // 애니메이션 리셋
        if (animHandler != null)
        {
            animHandler.PlayIdle();
        }
        
        // 콜라이더 활성화
        if (enemyCollider != null)
        {
            enemyCollider.enabled = true;
        }
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
    
    
    public void ForceKill()
    {
        if (isDead) return;
        
        TakeDamage(data.maxHp);
    }

    
    // public getters
    public EnemyType GetEnemyType => data?.type ?? EnemyType.Normal;
    public EnemyData GetEnemyData => data;
    public int GetCurrentHp => currentHp;
    public int GetMaxHp => data?.maxHp ?? 0;
}
