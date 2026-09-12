using NUnit.Framework;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BaseEntity))]
public class EnemyController : MonoBehaviour
{
    // --- COMPONENTES PRINCIPALES ---
    public StateMachine StateMachine { get; private set; }
    public Rigidbody2D Rb { get; private set; }
    public BaseEntity Health { get; private set; }
    public Animator Animator { get; private set; }
    public Collider2D Collider { get; private set; }
    public FlashEffectDamage FlashEffect;

    [Header("Targeting")]
    public Transform playerTarget;

    [Header("Sistema de Visión")]
    [Tooltip("Capa de las paredes/suelo que bloquearán la vista del enemigo")]
    public LayerMask capaObstaculosVision;

    [Header("Enemy Stats")]
    public float moveSpeed = 3f;
    public float detectionRange = 7f; 
    public float attackRange = 1.0f;
    public float longAttackRange = 2.0f;
    public bool isFlyingEnemy = false;
    public bool isRangeEnemy = false;

    [Header("Detección de Suelo")]
    public float groundCheckRadius = 0.2f;
    public PlataformaModular PlataformaActual { get; set; }

    [Header("Stun & Damage")]
    public float stunDuration = 2f; 
    public int contactDamage = 10;
    public float knockbackForceHurt = 5f;

    [Header("Combat Universals")]
    public int attackDamage = 15;
    public float attackWindup = 0.5f;   
    public float attackRecovery = 1.0f; 
    public Transform attackPoint;   
    public Transform blockPoint;    
    public LayerMask playerLayer;       
    public float timeBetweenAttacks = 1.5f;
    public bool isAttackingFar = false; 
    public bool unParryableAttacking = false;
    public bool unBlockableAttacking = false;
    protected float attackCooldownTimer;
    public float attackDetectorRange = 0.3f;
    public bool cannotBeInterrupted = false;
    public float selfBlockKnockbackForce = 10f; 
    public float blockKnockbackForce = 2f;
    public bool isBlocking;
    public float blockTime = 1f;

    [Header("Arena del Jefe (Opcional)")]
    public BossArenaTrigger arenaDelJefe;

    [Header("Game Feel & Visuals")]
    public float lightHitStopDuration = 0.07f;
    public GameObject hitSparkPrefab;

    [Header("Componentes Base")]
    public AudioSource bocinaSFX; 
    [SerializeField] private Transform nextGroundCheck;          
    [SerializeField] private float nextGroundCheckSize = 0.2f; 
    [SerializeField] private LayerMask groundLayer;

    [Header("Sonidos Base")]
    public AudioClip sonidoAtaque; // Renombrado de sonidoEspadazo
    public AudioClip fleshCut;

    [Header("Patrullaje Terrestre")]
    public float idleTime = 2f;    // Tiempo que se queda quieto
    public float patrolTime = 3f;  // Tiempo que camina antes de detenerse
    public float patrolSpeed = 2f; // Velocidad de patrullaje (suele ser menor que moveSpeed)

    // --- ESTADOS UNIVERSALES ---
    public EnemyIdleState IdleState { get; private set; }
    public EnemyChaseState ChaseState { get; private set; }
    public EnemyAttackState AttackState { get; private set; } // ¡Se queda aquí!
    public EnemyStunnedState StunnedState { get; private set; }
    public EnemyDeathState DeathState { get; private set; }
    public EnemyHurtState HurtState { get; private set; }
    public EnemyFallState FallState { get; private set; }
    public EnemyPatrolState PatrolState { get; private set; }

    protected virtual void Awake()
    {
        StateMachine = new StateMachine();
        Rb = GetComponent<Rigidbody2D>();
        Health = GetComponent<BaseEntity>();
        Animator = GetComponent<Animator>();
        Collider = GetComponent<Collider2D>();

        IdleState = new EnemyIdleState(this);
        ChaseState = new EnemyChaseState(this);
        AttackState = new EnemyAttackState(this);
        StunnedState = new EnemyStunnedState(this);
        DeathState = new EnemyDeathState(this);
        HurtState = new EnemyHurtState(this);
        FallState = new EnemyFallState(this);
        PatrolState = new EnemyPatrolState(this);

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) playerTarget = playerObj.transform;
    }

    protected virtual void Start()
    {
        StateMachine.Initialize(IdleState);
    }

    protected virtual void Update()
    {
        if (attackCooldownTimer > 0) attackCooldownTimer -= Time.deltaTime;
        StateMachine.CurrentState?.Update();
    }

    protected virtual void FixedUpdate()
    {
        StateMachine.CurrentState?.PhysicsUpdate();
    }

    private void OnEnable()
    {
        if (Health != null)
        {
            Health.AtBreakPosture += BreakGuard;
            Health.Dying += HandleDeath;
        }
    }

    private void OnDisable()
    {
        if (Health != null)
        {
            Health.AtBreakPosture -= BreakGuard;
            Health.Dying -= HandleDeath;
        }
    }

    private void BreakGuard() => StateMachine.ChangeState(StunnedState);
    private void HandleDeath() => StateMachine.ChangeState(DeathState);

    public bool CanAttack() => attackCooldownTimer <= 0f;
    public void ResetAttackCooldown() => attackCooldownTimer = timeBetweenAttacks;
    public bool IsThereGroundNext() => Physics2D.OverlapCircle(nextGroundCheck.position, nextGroundCheckSize, groundLayer) != null;
    public float DistanceToPlayer() => playerTarget == null ? Mathf.Infinity : Vector2.Distance(transform.position, playerTarget.position);
    
    public void FacePlayer()
    {
        if (playerTarget == null) return;
        if (playerTarget.position.x > transform.position.x && transform.localScale.x != 1f)
            transform.localScale = new Vector3(1f, 1f, 0);
        else if (playerTarget.position.x < transform.position.x && transform.localScale.x != -1f)
            transform.localScale = new Vector3(-1f, 1f, 0);
    }

    public virtual void IniciarAtaque()
    {
        // Por defecto no hace nada, o puedes poner un Debug.Log("El enemigo base ataca");
    }

    public bool GetRandomBoolean()
    {
        // Random.value genera un decimal entre 0.0 y 1.0.
        // Hay un 50% de probabilidad de que sea mayor a 0.5.
        return UnityEngine.Random.value > 0.5f;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("PlataformaMovil")) // Asegúrate de crear y ponerle este Tag a tus plataformas
        {
            PlataformaActual = collision.gameObject.GetComponent<PlataformaModular>();
        }
    }

    public bool IsGrounded()
    {
        // 1. Calculamos el punto central inferior del collider
        Vector2 bottomPosition = new Vector2(Collider.bounds.center.x, Collider.bounds.min.y);

        // 2. Trazamos el círculo invisible desde ese punto
        // (Necesitarás una variable float para el radio, ej: 0.2f)
        bool tocandoSuelo = Physics2D.OverlapCircle(bottomPosition, groundCheckRadius, groundLayer);

        return tocandoSuelo;
    }

    public bool CanSeePlayer()
    {
        if (playerTarget == null) return false;

        // 1. Verificación de Distancia Básica (El círculo original)
        float distance = DistanceToPlayer();
        if (distance > detectionRange) return false;

        // 2. Trazar la línea de visión
        Vector2 origenOjos = Collider.bounds.center;
        Vector2 posicionJugador = playerTarget.position;

        // Linecast dispara una línea exacta desde un punto A hasta un punto B.
        RaycastHit2D hitMuro = Physics2D.Linecast(origenOjos, posicionJugador, capaObstaculosVision);

        // Si es null, el láser llegó al jugador sin tocar ninguna pared
        if (hitMuro.collider == null)
        {
            Debug.DrawLine(origenOjos, posicionJugador, Color.yellow);
            return true;
        }

        // Si tocó algo de la capa de obstáculos, la visión está bloqueada
        Debug.DrawLine(origenOjos, hitMuro.point, Color.red);
        return false;
    }

    public virtual void EjecutarSiguienteMovimiento()
    {
        // Por defecto no hace nada, o puedes poner un Debug.Log("El enemigo base ataca");
    }

    public virtual bool IntentarAtaqueLargo()
    {
        return false;
    }

    // Hechos VIRTUALES para que los hijos puedan añadir cosas
    public virtual void AnimationEvent_OnImpact()
    {
        
    }

    public virtual void AnimationEvent_OnEnd()
    {
        if (StateMachine.CurrentState == AttackState) AttackState.OnAnimationEndFrame();
        if (StateMachine.CurrentState == HurtState) HurtState.AnimationFinishTrigger();
    }

    public virtual void DeathBoss_OnEnd()
    {
        if (arenaDelJefe != null)
        {
            arenaDelJefe.FinalizarPeleaDeJefe();
        }
    }

    public virtual void AnimationEvent_WindupFinished()
    {
        StateMachine.CurrentState.WindupFinishedTrigger();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null && player.StateMachine.CurrentState != player.HurtState)
            {
                player.GetAttack(contactDamage, transform);
                float direccionEmpuje = (player.transform.position.x > transform.position.x) ? 1f : -1f;
                player.Rb.linearVelocity = new Vector2(direccionEmpuje * knockbackForceHurt, player.Rb.linearVelocity.y);
            }
        }
    }

    public void ReproducirSonido(AudioClip audio) => bocinaSFX.PlayOneShot(audio);

    // MÉTODO VIRTUAL: Solo hace el daño base. El hijo le pondrá el Parry encima.
    public virtual void GetAttack(int quantityDamage, Transform attacker, bool isThrowAttack=false, float directionThrowAttack=1.0f)
    {            
        if (hitSparkPrefab != null)
        {
            GameObject.Instantiate(hitSparkPrefab, transform.position, Quaternion.identity);
        }
        ReproducirSonido(fleshCut);
        if (CameraController.Instancia != null)
        {
            CameraController.Instancia.AgitarCamara(0.1f, 0.04f);
        }
                
        if (StateMachine.CurrentState == StunnedState)
        {
            Debug.Log("aplicando dano finisher");
            Health.FinisherDamage();
            if (Health.finisherLimit > 0) StateMachine.ChangeState(HurtState);
            return;
        }

        if (cannotBeInterrupted)
        {
            FlashEffect.EjecutarFlasheo();
            Health.Damage(quantityDamage);
            return;
        }
        StateMachine.ChangeState(HurtState);
        Health.Damage(quantityDamage);
    }

    public void TriggerHitStop(float duration) => StartCoroutine(HitStopRoutine(duration));

    private System.Collections.IEnumerator HitStopRoutine(float duration)
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.purple;
        if (attackPoint != null)
        {
            Gizmos.DrawWireSphere(attackPoint.position, attackDetectorRange);
        }
    }
}