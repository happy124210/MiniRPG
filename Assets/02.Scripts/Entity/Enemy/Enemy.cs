using UnityEngine;

public class Enemy : MonoBehaviour, IDamagable
{
    [Header("Stat")]
    [SerializeField] private EnemyData data;
    private int currentHp;
    private bool isDead;
    public bool IsDead => isDead;
    
    [Header("Animation")]
    [SerializeField] private AnimatorOverrideController overrideController;
    [SerializeField] private EnemyAnimationHandler animHandler;
    private Animator animator;

    private void Awake()
    {
        animHandler = GetComponent<EnemyAnimationHandler>();
        
        animHandler.OnDieEndCallback -= OnDie;
        animHandler.OnDieEndCallback += OnDie;
    }

    private void Start()
    {
        animHandler.SetOverrideController(data.overrideController);
        
        currentHp = data.maxHp;
        animHandler.PlayIdle();
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
        isDead = true;
        animHandler.StopIdle();
        animHandler.PlayDie();
    }
    
    private void OnDie()
    {
        Destroy(gameObject);
    }

    public EnemyType GetEnemyType => data.type;
}
