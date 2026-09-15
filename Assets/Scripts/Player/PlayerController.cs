using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.Rendering;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Stats")]
    public float moveSpeed = 8f;
    public float jumpForce = 12f;
    public PlataformaModular PlataformaActual { get; set; }

    [Header("Dash Stats")]
    public float dashSpeed = 20f;      // Mucho más rápido que caminar
    public float dashDuration = 0.2f;  // Fracción de segundo
    public float dashCooldown = 1f;    // Cuánto debe esperar para usarlo de nuevo

    public float LastDashTime { get; set; } = -100f; // Para calcular el cooldown
    public bool DashInput { get; set; }
    public bool HealInput { get; set; }
    public bool ThrowAttackInput {get; set;}
    public bool InteractInput {get; set; }
    public PlayerDashState DashState { get; private set; }

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;          // El punto vacío que irá en los pies
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.8f, 0.2f); // El tamaño de la caja de detección
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask platformLayer;
    private Coroutine rutinaCaida;

    [Header("Wall Check & Jump")]
    [SerializeField] private Transform wallCheck; 
    [SerializeField] private Vector2 wallCheckSize = new Vector2(0.2f, 0.8f);
    [SerializeField] private LayerMask wallLayer;
    public float wallSlideSpeed = 1f;
    public Vector2 wallJumpForce = new Vector2(5f, 10f); // X te empuja lejos, Y te empuja arriba
    [Header("Configuración de Paredes")]
    public float anguloMaximoPendiente = 45f;

    [Header("Visuals (Wall Slide)")]
    public SpriteRenderer spriteRenderer; // Arrastra el componente SpriteRenderer aquí en el Inspector
    
    // Interruptor para bloquear el flip cuando estemos en la pared
    public bool CanFlip { get; set; } = true;

    [Header("Combat Stats")]
    public float comboWindow = 0.8f;
    public int[] lightAttackDamages = new int[] { 10, 15, 25 }; // Cuánto daño hace el ataque débil
    public float heavyAttackDuration = 0.6f; // Tarda el doble que el ligero
    public int heavyAttackDamage = 40;       // Hace mucho más daño
    public int maxComboSteps = 3; // Este valor puede aumentar con mejoras en el juego
    public int comboStep = 0;
    public int throwableWeaponLimit = 3;
    public int throwableWeaponCounter;

    [Header("Defensive Stats")]
    public float parryDuration = 0.5f; // La "Ventana" de tiempo perfecta (muy corta)

    public bool BlockInput { get; set; }
    public bool IsBlockButtonHeld { get; set; }
    public PlayerBlockState BlockState { get; private set; }
    public BaseEntity Health { get; private set; } // Referencia a su propia salud

    [Header("Stun / Posture")]
    public float stunDuration = 1.5f; // Tiempo que el jugador queda vulnerable
    public PlayerStunnedState StunnedState { get; private set; }
    public bool isStunned = false;
    public bool beingPushed = false;
    public bool pushedUp = false;
    public float hurtAndPushDirection;

    [Header("Jump Game Feel")]
    public float defaultGravity = 2f; 
    public float fallGravityMultiplier = 3.5f; // Cae más rápido de lo que sube
    public float hangTimeThreshold = 1.5f; // El rango de velocidad vertical que consideramos "la cima"
    public float hangTimeGravityMultiplier = 0.5f; // Gravedad reducida (flotar) en la cima
    public float gravedadOriginal;

    [Header("Game Feel")]
    // 0.05 segundos es el estándar para golpes ligeros.
    // Para golpes pesados, puedes usar 0.1 o 0.15.
    public float lightHitStopDuration = 0.5f;

    [Header("Coyote Time")]
    public float coyoteTime = 0.15f; // 0.15 segundos es el estándar de la industria
    public float coyoteTimer;

    [Header("Combat Detection")]
    public Transform attackPoint;
    public Transform heavyAttackPoint;  
    public Vector2 heavyAttackSize = new Vector2(0.2f,0.3f);
    public float attackRange = 0.5f;   // El radio del círculo de daño
    public LayerMask enemyLayers;      // Para no golpearnos a nosotros mismos o al suelo

    [Header("Sistema de Trampas (Hazards)")]
    public Vector2 ultimoPuntoSeguro;
    public int danoPorPinchos = 35; // La cantidad "considerable" que quieres quitar
    
    // Un pequeño temporizador para no guardar la posición cada milisegundo
    private float temporizadorGuardado;

    [Header("Thowing Weapons")]
    public GameObject shurikenPrefab;

    [Header("Visual Effects")]
    public GameObject hitSparkPrefab;
    public GameObject hitSparkParryPrefab; // Arrastra tu Prefab del chispazo aquí

    [Header("Componentes")]
    public AudioSource bocinaSFX; // Arrastra el AudioSource de tu jugador aquí en el Inspector

    [Header("Sonidos")]
    public AudioClip sonidoEspadazo;
    public AudioClip fleshCut;
    public AudioClip parrySound;
    public AudioClip blockSound;

    // Referencias públicas (con set privado) para que los Estados puedan usarlas
    public Rigidbody2D Rb { get; private set; }
    public StateMachine StateMachine { get; private set; }
    public Vector2 MovementInput { get; private set; }
    public bool JumpInput { get; set; }
    public bool JumpReleaseInput { get; set; } // Set público para que los estados puedan apagarlo al consumirlo
    public bool IsFacingRight { get; private set; } = true;
    public bool LightAttackInput { get; set; }
    public PlayerWallSlideState WallSlideState { get; private set; }
    public PlayerWallJumpState WallJumpState { get; private set; }

    // Instancias de nuestros estados concretos
    public PlayerIdleState IdleState { get; private set; }
    public PlayerMoveState MoveState { get; private set; }
    public PlayerJumpState JumpState { get; private set; }
    public PlayerFallState FallState { get; private set; }
    public PlayerLightAttackState LightAttackState { get; private set; }
    public bool HeavyAttackInput { get; set; }
    public PlayerHeavyAttackState HeavyAttackState { get; private set; }
    public PlayerHurtState HurtState { get; private set; }
    public PlayerDeathState DeathState { get; private set; }
    public PlayerHealState HealState { get; private set; }
    public PlayerThrowAttackState ThrowAttackState { get; private set; }

    // El sistema de inputs que generamos en Unity
    private PlayerControls controls;

    public Animator Animator { get; private set; }

    private void Awake()
    {
        Rb = GetComponent<Rigidbody2D>();
        StateMachine = new StateMachine();
        Application.targetFrameRate = 60;
        throwableWeaponCounter = throwableWeaponLimit;
        gravedadOriginal = Rb.gravityScale;

        // Inicializamos los estados y les pasamos 'this' (este mismo script) como contexto
        IdleState = new PlayerIdleState(this);
        MoveState = new PlayerMoveState(this);
        JumpState = new PlayerJumpState(this);
        FallState = new PlayerFallState(this);
        LightAttackState = new PlayerLightAttackState(this);
        HeavyAttackState = new PlayerHeavyAttackState(this);
        Health = GetComponent<BaseEntity>();
        BlockState = new PlayerBlockState(this);
        DashState = new PlayerDashState(this);
        StunnedState = new PlayerStunnedState(this);
        HurtState = new PlayerHurtState(this);
        DeathState = new PlayerDeathState(this);
        WallSlideState = new PlayerWallSlideState(this);
        WallJumpState = new PlayerWallJumpState(this);
        HealState = new PlayerHealState(this);
        ThrowAttackState = new PlayerThrowAttackState(this);

        controls = new PlayerControls();
        Animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        controls.Gameplay.Enable();
        controls.Gameplay.Jump.started += OnJumpStarted;
        controls.Gameplay.Jump.canceled += OnJumpCanceled;
        controls.Gameplay.LightAttack.started += OnLightAttack;
        controls.Gameplay.HeavyAttack.started += OnHeavyAttack;
        controls.Gameplay.Block.started += OnBlockStarted;
        controls.Gameplay.Block.canceled += OnBlockCanceled;
        controls.Gameplay.Dash.started += OnDash;
        controls.Gameplay.Heal.started += OnHeal;
        controls.Gameplay.ThrowAttack.started += OnThrowAttack;
        controls.Gameplay.Interact.started += OnInteract;
        if (Health != null)
        {
            Health.AtBreakPosture += BreakPosturePlayer;
            Health.Dying += HandleDeath;
        }
    }

    private void OnDisable()
    {
        controls.Gameplay.Disable();
        controls.Gameplay.Jump.started -= OnJumpStarted;
        controls.Gameplay.Jump.canceled -= OnJumpCanceled;
        controls.Gameplay.LightAttack.started -= OnLightAttack;
        controls.Gameplay.HeavyAttack.started -= OnHeavyAttack;
        controls.Gameplay.Block.started -= OnBlockStarted;
        controls.Gameplay.Block.canceled -= OnBlockCanceled;
        controls.Gameplay.Dash.started -= OnDash;
        controls.Gameplay.Heal.started -= OnHeal;
        controls.Gameplay.ThrowAttack.started -= OnThrowAttack;
        controls.Gameplay.Interact.started -= OnInteract;
        if (Health != null)
        {
            Health.AtBreakPosture -= BreakPosturePlayer;
            Health.Dying -= HandleDeath;
        }
    }

    private void Start()
    {
        // Arrancamos la máquina de estados en el estado Idle
        StateMachine.Initialize(IdleState);
    }

    private void Update()
    {
        // 1. EL RELOJ DEL COYOTE TIME (Limpio y automático)
        if (IsGrounded()) // Usa el método o variable que ya tengas para saber si pisas el suelo
        {
            coyoteTimer = coyoteTime; // Mientras pises el suelo, el reloj está al máximo
        }
        else
        {
            coyoteTimer -= Time.deltaTime; // Al caer del borde, el reloj empieza a agotarse
        }

        // Leemos el input constantemente para que los estados lo tengan disponible
        MovementInput = controls.Gameplay.Movement.ReadValue<Vector2>();

        // Verificamos si necesitamos voltear al personaje
        if (CanFlip)
        {
            CheckFlip();
        }

        HandleGravity(); // Evaluamos la gravedad en cada frame
        
        // Si el joystick/tecla apunta hacia abajo (menos de -0.5 para evitar toques accidentales)
        // y se presionó el botón de salto:
        if (MovementInput.y < -0.5f && JumpInput)
        {
            JumpInput = false; // "Consumimos" el salto para que el StateMachine no lo use para saltar hacia arriba
            IntentarDejarseCaer();
        }

        // Guardamos la posición cada 0.5 segundos para asegurarnos de que es una plataforma estable
        temporizadorGuardado -= Time.deltaTime;

        if (temporizadorGuardado <= 0f)
        {
            // Validamos que sea un lugar seguro: tocando el suelo, sin caer/subir, y sin deslizarse en muros
            if (IsGrounded() && Mathf.Abs(Rb.linearVelocity.y) < 0.1f && !IsTouchingWall() && !IsGroundedButPlatform())
            {
                ultimoPuntoSeguro = transform.position;
                temporizadorGuardado = 0.5f; // Reiniciamos el reloj
            }
        }

        // Le decimos a la máquina de estados que ejecute el Update del estado actual
        StateMachine.CurrentState?.Update();
    }

    private void FixedUpdate()
    {
        // Ejecutamos las físicas del estado actual
        StateMachine.CurrentState?.PhysicsUpdate();
    }

    // Callback del nuevo Input System de Unity
    private void OnJumpStarted(InputAction.CallbackContext context)
    {
        JumpInput = true;
    }

    private void OnJumpCanceled(InputAction.CallbackContext context)
    {
        JumpReleaseInput = true;
    }

    private void OnLightAttack(InputAction.CallbackContext context)
    {
        LightAttackInput = true;
    }

    private void OnHeal(InputAction.CallbackContext context)
    {
        if (StateMachine.CurrentState == IdleState)
        {
            HealInput = true;
        }
    }

    private void OnThrowAttack(InputAction.CallbackContext context)
    {
        if (throwableWeaponCounter > 0)
        {
            ThrowAttackInput = true;
        }
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        Debug.Log("interact presionado");
        InteractInput = true;
    }

    private void OnHeavyAttack(InputAction.CallbackContext context)
    {
        if(StateMachine.CurrentState != HeavyAttackState)
        {
            HeavyAttackInput = true;
        }
    }

    private void BreakPosturePlayer()
    {
        StateMachine.ChangeState(StunnedState);
    }

    public void RecibirDanoPorTrampa()
    {
        StartCoroutine(SecuenciaDanoTrampa());
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("PlataformaMovil")) // Asegúrate de crear y ponerle este Tag a tus plataformas
        {
            PlataformaActual = collision.gameObject.GetComponent<PlataformaModular>();
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("PlataformaMovil"))
        {
            PlataformaActual = null;
        }
    }

    private IEnumerator SecuenciaDanoTrampa()
    {
        // 1. Aplicamos el daño (aquí llamarías a tu sistema de vida, ej: vidaActual -= danoPorPinchos)
        Debug.Log("¡Auch! Pierdes vida por los pinchos.");
        // 2. Detenemos cualquier inercia o fuerza de físicas que llevara el jugador
        Rb.linearVelocity = Vector2.zero;
        Rb.gravityScale = 0f; // ¡Tu idea aplicada! El jugador flota en el aire.
        StateMachine.ChangeState(HurtState);
        Health.EnviromentDamage(danoPorPinchos);

        yield return new WaitForSeconds(0.20f);
        
        
        if(Health.ActualHealth > 0)
        {
            // 3. ¡La teletransportación!
            transform.position = ultimoPuntoSeguro;
            Rb.gravityScale = gravedadOriginal;
            // 4. Forzamos a la máquina de estados a volver a Idle para reiniciar cualquier animación de salto/caída
            StateMachine.ChangeState(IdleState);
        }
        // (Opcional) Aquí podrías darle 1 segundo de invulnerabilidad para que no reciba daño extra al reaparecer
    }

    private void IntentarDejarseCaer()
    {
        // 1. Usamos tu radar de suelo para ver qué estamos pisando
        Collider2D sueloActual = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);

        // 2. Verificamos si encontramos suelo Y si ese suelo es una plataforma flotante
        if (sueloActual != null && sueloActual.CompareTag("OneWayPlatform"))
        {
            // Si ya estábamos cayendo por otra, la detenemos
            if (rutinaCaida != null)
            {
                StopCoroutine(rutinaCaida);
            }
            
            // Iniciamos la caída libre
            rutinaCaida = StartCoroutine(AtravesarPlataforma(sueloActual));
        }
    }

    private IEnumerator AtravesarPlataforma(Collider2D plataforma)
    {
        // 1. Apagamos las colisiones ENTRE el jugador y esa plataforma específica
        // (Los enemigos seguirán pisándola sin caerse)
        Physics2D.IgnoreCollision(GetComponent<Collider2D>(), plataforma, true);

        // 2. Esperamos medio segundo (tiempo suficiente para que el jugador cruce hacia abajo)
        yield return new WaitForSeconds(0.4f);

        // 3. Volvemos a encender las colisiones para que la plataforma vuelva a ser sólida
        Physics2D.IgnoreCollision(GetComponent<Collider2D>(), plataforma, false);
    }

    private void HandleDeath()
    {
        StateMachine.ChangeState(DeathState);
    }

    public int GetLightAttackDamage()
    {
        // comboStep empieza en 1, así que le restamos 1 para que encaje con los índices del arreglo (0, 1, 2...)
        int indice = comboStep - 1;

        // Prevención de errores (Programación Defensiva): 
        // Si el índice es menor a 0 por algún motivo, devolvemos el primer daño.
        if (indice < 0) return lightAttackDamages[0];

        // Si el jugador consiguió un upgrade de 5 combos, pero se te olvidó añadir 
        // más números al arreglo en Unity, esto evita que el juego crashee.
        // Simplemente repetirá el daño del último golpe configurado.
        if (indice >= lightAttackDamages.Length) return lightAttackDamages[lightAttackDamages.Length - 1];

        // Si todo está correcto, devolvemos el daño correspondiente al golpe actual
        return lightAttackDamages[indice];
    }

    

    public bool IsGrounded()
    {
        // Lanza una caja invisible en la posición de 'groundCheck'. 
        // Devuelve 'true' si colisiona con algo que esté en la 'groundLayer'.
        return Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);
    }

    public bool IsGroundedButPlatform()
    {
        return Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, platformLayer);
    }

    public bool IsTouchingWall()
    {
            // 1. Calculamos hacia dónde está mirando el jugador 
        // (Ajusta esto si giras a tu personaje usando otra técnica como un bool o rotación)
        Vector2 direccionMirada = transform.localScale.x > 0 ? Vector2.right : Vector2.left;

        // 2. Lanzamos tu misma caja, empujándola una distancia mínima (0.1f) hacia el frente
        RaycastHit2D hit = Physics2D.BoxCast(wallCheck.position, wallCheckSize, 0f, direccionMirada, 0.1f, wallLayer);

        if (hit.collider != null)
        {
            // 3. Obtenemos el ángulo de la superficie contra la que chocamos
            float anguloSuperficie = Vector2.Angle(hit.normal, Vector2.up);

            // 4. Si el ángulo supera el máximo (ej. una pared de 90 grados), es un muro real.
            // Si es una diagonal de 45 o 30 grados, devolverá false permitiendo al jugador caminar.
            return anguloSuperficie > anguloMaximoPendiente;
        }

        return false;
    }

    private void HandleGravity()
    {
        // 1. ¿Estamos cerca del punto más alto del salto? (Velocidad Y casi en cero)
        if (Mathf.Abs(Rb.linearVelocity.y) < hangTimeThreshold)
        {
            // Reducimos la gravedad para crear el "Hang Time" (flotar)
            Rb.gravityScale = defaultGravity * hangTimeGravityMultiplier;
        }
        // 2. ¿Estamos cayendo? (Velocidad Y negativa)
        else if (Rb.linearVelocity.y < 0f)
        {
            // Multiplicamos la gravedad para un aterrizaje rápido y con peso
            Rb.gravityScale = defaultGravity * fallGravityMultiplier;
        }
        // 3. ¿Estamos subiendo con fuerza o en el suelo?
        else
        {
            Rb.gravityScale = defaultGravity;
        }
    }

    // Esto es opcional pero súper útil: dibuja la caja en el editor para que puedas verla
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
        }
        if (wallCheck != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(wallCheck.position, wallCheckSize);
        }
        if(heavyAttackPoint != null)
        {
            Gizmos.color = Color.purple;
            Gizmos.DrawWireCube(heavyAttackPoint.position, heavyAttackSize);
        }
    }

    private void CheckFlip()
    {
    // Si nos movemos a la derecha pero NO estamos mirando a la derecha, volteamos
        if (MovementInput.x > 0.1f && !IsFacingRight)
        {
            Flip();
        }
        // Si nos movemos a la izquierda pero SÍ estamos mirando a la derecha, volteamos
        else if (MovementInput.x < -0.1f && IsFacingRight)
        {
            Flip();
        }
    }

    public void Flip()
    {
        // Invertimos el estado de la variable
        IsFacingRight = !IsFacingRight;

        // Rotamos todo el GameObject 180 grados en el eje Y
        transform.localScale = new Vector3(-1f*transform.localScale.x, 1f, 0f);
    }

    public void AnimationEvent_OnEnd()
    {
        // Le avisa al estado actual que su animación visual ya terminó
        StateMachine.CurrentState.AnimationFinishTrigger();
    }

    public virtual void AnimationEvent_WindupFinished()
    {
        StateMachine.CurrentState.WindupFinishedTrigger();
    }

    private void OnBlockStarted(InputAction.CallbackContext context)
    {
        IsBlockButtonHeld = true;
    }

    private void OnBlockCanceled(InputAction.CallbackContext context)
    {
        IsBlockButtonHeld = false;
    }
    private void OnDash(InputAction.CallbackContext context)
    {
        DashInput = true;
    }

    // Unity llamará a este método mediante un Animation Event
    public void AnimationEvent_OnImpact()
    {
        // Le avisamos al estado actual que es el momento de aplicar el daño
        StateMachine.CurrentState.AnimationImpactTrigger();
    }

    // Este es el método que llamaremos desde el estado de ataque
    public void TriggerHitStop(float duration)
    {
        StartCoroutine(HitStopRoutine(duration));
    }

    public void ReproducirSonido(AudioClip audio)
    {
        // ¡El truco de PlayOneShot!
        bocinaSFX.PlayOneShot(audio);
    }

    // La magia de congelar el tiempo
    private System.Collections.IEnumerator HitStopRoutine(float duration)
    {
        // 1. Congelamos el tiempo del motor
        Time.timeScale = 0f;

        // 2. Esperamos en TIEMPO REAL (ignorando que el juego está pausado)
        yield return new WaitForSecondsRealtime(duration);

        // 3. Devolvemos el tiempo a la normalidad
        Time.timeScale = 1f;
    }

    // NUEVO MÉTODO: El Jugador procesa los golpes que recibe
    public void GetAttack(int quantityDamage, Transform attacker)
    {
        EnemyController attackerBaseComponent = attacker.GetComponent<EnemyController>();
        // 1. PARRY PERFECTO
        if (Health.IsParrying && !attackerBaseComponent.unParryableAttacking)
        {
            Debug.Log("Jugador: ¡REESCRIBIÓ EL TIEMPO! Parry perfecto.");
            if (hitSparkParryPrefab != null)
            {
                // Instanciamos el chispazo. Puedes sumarle un pequeño Offset a enemigoTransform.position 
                // si quieres que salga un poco más arriba, en el pecho y no en los pies.
                if (!attackerBaseComponent.isFlyingEnemy && !attackerBaseComponent.isRangeEnemy)
                {
                    Instantiate(hitSparkParryPrefab, attackerBaseComponent.attackPoint.position, Quaternion.identity);
                }
                else
                {
                    Instantiate(hitSparkParryPrefab, transform.position, Quaternion.identity);
                }
            }
            if (CameraController.Instancia != null)
            {
                CameraController.Instancia.AgitarCamara(0.1f, 0.04f);
            }
            ReproducirSonido(parrySound);
            Animator.Play("Parry");
            // Le mandamos un estímulo a EntidadBase para que dispare eventos de sonido/partículas
            Health.Damage(0); 

            // Dañamos la postura del atacante (Enemigo)
            BaseEntity saludAtacante = attacker.GetComponent<BaseEntity>();
            if (saludAtacante != null)
            {
                // DOWNCASTING: Preguntamos si el atacante es un Espadachín
                SwordsmanController swordsman = attacker.GetComponent<SwordsmanController>();
                
                // Si es espadachín leemos su variable, si no (ej. es una bestia), usamos 50 por defecto
                int postureDamage = swordsman != null ? swordsman.postureDamageOnParried : 50;
                
                saludAtacante.PostureDamage(postureDamage);
            }

            return; // No recibe daño ni retroceso
        }

        // 2. BLOQUEO NORMAL
        if (Health.IsBlocking && !attackerBaseComponent.unBlockableAttacking && !attackerBaseComponent.unParryableAttacking)
        {
            Debug.Log("Jugador: Bloqueó el golpe y retrocede.");

            // EntidadBase se encarga de restarle a la POSTURA del jugador
            ReproducirSonido(blockSound);
            Animator.Play("Parry");
            Health.PostureDamage(quantityDamage/2);

            // APLICAMOS EL KNOCKBACK AL JUGADOR
            if (Rb != null)
            {
                Debug.Log("Se esta aplicando knockback");
                // Si el enemigo está a la derecha, nos empuja a la izquierda (-1), y viceversa (1)
                float pushDirection = (transform.localScale.x < attacker.localScale.x) ? 1f : -1f;
                Debug.Log(pushDirection);
                // DOWNCASTING: Leemos la fuerza de retroceso si es un espadachín
                SwordsmanController swordsman = attacker.GetComponent<SwordsmanController>();
                float force = swordsman != null ? swordsman.blockKnockbackForce : 2f;

                Rb.linearVelocity = new Vector2(pushDirection * force, Rb.linearVelocity.y);
            }

            return; // No pierde vida
        }

        Debug.Log("Dano recibido a jugador");
        // 3. DAÑO DIRECTO (Sin defensas)
        if (isStunned)
        {
            Health.Damage(Health.ActualHealth);
            return;
        }
        
        if (Rb != null)
        {
            // Si el enemigo está a la derecha, nos empuja a la izquierda (-1), y viceversa (1)
            float pushDirection = (transform.localScale.x < attacker.localScale.x) ? 1f : -1f;
            
            // DOWNCASTING: Leemos la fuerza de retroceso si es un espadachín
            SwordsmanController swordsman = attacker.GetComponent<SwordsmanController>();
            HurtState.hitDirection = pushDirection;
            HurtState.swordsman = swordsman; 
        }
        StateMachine.ChangeState(HurtState);
        Health.Damage(quantityDamage);
    }
}
