using UnityEngine;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    public StateMachine StateMachine {get; private set;}
    public PlayerAnimationHandler AnimationHandler {get; private set;}
    public StatHandler StatHandler {get; private set;}
    
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    
    [field: Header("Combat")]
    [SerializeField] public float AttackInterval { get; private set; } = 1f;

    public Transform targetEnemy;
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
    
    public bool CanMove() => true;

    public bool IsEnemyInRange()
    {
        return targetEnemy
               && Vector3.Distance(transform.position, targetEnemy.position) <= detectRange;
    }

    public void DetectEnemy()
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position, detectRange, LayerMask.GetMask("Enemy"));

        targetEnemy = enemies.Length > 0 
            ? enemies[0].transform 
            : null;
    }
}