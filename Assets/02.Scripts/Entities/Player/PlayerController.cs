using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public StateMachine StateMachine {get; private set;}
    public PlayerAnimationHandler AnimationHandler {get; private set;}
    public StatHandler StatHandler {get; private set;}
    
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    
    [field: Header("Combat")]
    [SerializeField] public float AttackInterval { get; private set; } = 1f;

    private float lastDetectTime;
    private const float DETECT_INTERVAL = 0.1f;

    public bool IsMoving { get; set; } = true;
    [SerializeField] private Transform targetEnemy;
    [SerializeField] private float detectRange = 5f;
    
    

    private void Awake()
    {
        StateMachine = new StateMachine();
        AnimationHandler = GetComponent<PlayerAnimationHandler>();
        StatHandler = GetComponent<StatHandler>();
    }

    private void Start()
    {
        StateMachine.ChangeState(new PlayerIdleState(this));
    }

    private void Update()
    {
        StateMachine.Update();
    }

    public void MoveForward()
    {
        if (!IsMoving) return;
        
        transform.Translate(Vector3.forward * (Time.deltaTime * moveSpeed));
    }

    public void PerformAttack()
    {
        if (!targetEnemy) return;

        IDamagable target = targetEnemy.GetComponent<IDamagable>();
        if (target == null) return;
        
        int damage = StatHandler.GetStat(StatType.AttackPower);
        target.TakeDamage(damage);
    }

    public void OnDeath()
    {
        // TODO
        // 사망 처리
        
        Debug.Log("Player dead");
    }
    
    public bool CanMove() => IsMoving;

    public bool IsEnemyInRange()
    {
        if (targetEnemy == null) return false;

        Enemy enemy = targetEnemy.GetComponent<Enemy>();
        if (enemy && enemy.IsDead) return false;

        return Vector3.Distance(transform.position, targetEnemy.position) <= detectRange;
    }

    public void DetectEnemy()
    {
        // 0.1f마다 탐지
        if (Time.time - lastDetectTime < DETECT_INTERVAL) return;
        lastDetectTime = Time.time;
        
        Collider[] enemies = Physics.OverlapSphere(transform.position, detectRange, LayerMask.GetMask("Enemy"));

        foreach (var col in enemies)
        {
            Enemy enemy = col.GetComponent<Enemy>();
            if (!enemy || enemy.IsDead) continue;
            
            targetEnemy = enemy.transform;
            return;
        }
    }
}