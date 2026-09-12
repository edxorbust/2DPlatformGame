using UnityEngine;

public class FlyingEnemyController : EnemyController // Heredamos de tu controlador base
{
    [Header("Patrulla (Movimiento Infinito)")]
    public Transform puntoDestino;
    private Vector2 posicionA;
    private Vector2 posicionB;
    private Vector2 destinoPatrulla;

    [Header("Combate Aéreo")]
    public GameObject bolaDeFuegoPrefab;
    [Tooltip("Qué tan alto se mantendrá por encima del jugador")]
    public float alturaDeVuelo = 2.5f;
    [Tooltip("Distancia X para acercarse y disparar")]
    public float distanciaAtaqueX = 3f;
    public float distanciaAtaqueY = 3f;
    [Tooltip("Distancia X para alejarse tras disparar")]
    public float distanciaRetiradaX = 6f;
    public float distanciaRetiradaY = 6f;
    public float velocidadVuelo = 4f;

    // Variables internas de combate
    private Vector2 objetivoCombate;
    private Vector2 posicionGuardadaJugador;
    private bool estaRetirandose = false;

    protected override void Start()
    {
        base.Start(); // Inicializa la máquina de estados y componentes del padre
        
        // Aseguramos que sea completamente manejado por código
        //Rb.bodyType.isKinematic = true; 
        // Configuramos los puntos de patrulla al igual que tu plataforma móvil
        isFlyingEnemy = true;
        posicionA = transform.position;
        if (puntoDestino != null)
        {
            posicionB = puntoDestino.position;
            destinoPatrulla = posicionB;
        }
    }

    protected override void FixedUpdate()
    {
        // Ejecuta el PhysicsUpdate de los estados
        //base.FixedUpdate(); 

        // Si está en Hurt o Death, no calculamos movimiento
        if (StateMachine.CurrentState == HurtState || StateMachine.CurrentState == DeathState) return;

        // Máquina de decisiones de movimiento
        if (StateMachine.CurrentState == IdleState)
        {
            Patrullar();
            
            // Si ve al jugador, pasa a modo combate (usamos ChaseState para el posicionamiento)
            if (DistanceToPlayer() <= detectionRange)
            {
                estaRetirandose = false;
                StateMachine.ChangeState(ChaseState);
            }
        }
        else if (StateMachine.CurrentState == ChaseState)
        {
            PosicionarseParaCombate();
        }
    }

    // --- LÓGICA DE PATRULLA ---
    private void Patrullar()
    {
        if (puntoDestino == null) return;

        transform.position = Vector2.MoveTowards(transform.position, destinoPatrulla, moveSpeed * Time.fixedDeltaTime);
        
        // Voltear el sprite según hacia dónde vuela
        if (destinoPatrulla.x > transform.position.x && transform.localScale.x != 1f)
            transform.localScale = new Vector3(1f, 1f, 0);
        else if (destinoPatrulla.x < transform.position.x && transform.localScale.x != -1f)
            transform.localScale = new Vector3(-1f, 1f, 0);

        // Si llegó al punto, intercambiamos destinos
        if (Vector2.Distance(transform.position, destinoPatrulla) < 0.1f)
        {
            destinoPatrulla = (destinoPatrulla == posicionA) ? posicionB : posicionA;
        }
    }


    // --- LÓGICA DE POSICIONAMIENTO AÉREO ---
    private void PosicionarseParaCombate()
    {
        if (playerTarget == null) return;

        // Calculamos a qué lado del jugador estamos para mantener la distancia correcta
        float direccionX = transform.position.x > playerTarget.position.x ? 1f : -1f;
        float distanciaXActual = estaRetirandose ? distanciaRetiradaX : distanciaAtaqueX;
        float distanciaYActual = estaRetirandose ? distanciaRetiradaY : distanciaAtaqueY;


        // El objetivo siempre está 'alturaDeVuelo' por encima del jugador, y 'distanciaX' a los lados
        objetivoCombate = new Vector2(
            playerTarget.position.x + (distanciaXActual * direccionX),
            playerTarget.position.y + distanciaYActual
        );

        transform.position = Vector2.MoveTowards(transform.position, objetivoCombate, velocidadVuelo * Time.fixedDeltaTime);
        FacePlayer(); // Miramos siempre al jugador mientras nos movemos

        // ¿Llegamos al punto para disparar y no estamos en enfriamiento?
        if (!estaRetirandose && Vector2.Distance(transform.position, objetivoCombate) < 0.2f && CanAttack())
        {
            StateMachine.ChangeState(AttackState);
        }
        // Si estábamos retirándonos, ya llegamos lejos y recargamos, volvemos a la ofensiva
        else if (estaRetirandose && CanAttack())
        {
            estaRetirandose = false;
        }
        
        // Si el jugador huye y sale del rango de detección, volvemos a patrullar
        if (DistanceToPlayer() > detectionRange * 1.5f)
        {
            StateMachine.ChangeState(IdleState);
        }
    }

    // --- LÓGICA DE ATAQUE (Sobrescribiendo al padre) ---
    
    public override void IniciarAtaque()
    {
        // Se llama en el Enter() de tu AttackState
        // Aquí ocurre el "Windup": guardamos la posición exacta que tiene el jugador ahora mismo
        if (playerTarget != null)
        {
            posicionGuardadaJugador = playerTarget.position;
        }
    }

    public override void AnimationEvent_OnImpact()
    {
        // Se llama en el frame exacto de la animación donde escupe/dispara
        if (bolaDeFuegoPrefab != null)
        {
            // Creamos la bola de fuego
            GameObject fuego = Instantiate(bolaDeFuegoPrefab, transform.position, Quaternion.identity);
            
            // Calculamos la dirección matemática hacia el punto guardado
            Vector2 direccionDisparo = (posicionGuardadaJugador - (Vector2)transform.position).normalized;
            
            FireBall scriptProyectil = fuego.GetComponent<FireBall>();
            if (scriptProyectil != null)
            {
                scriptProyectil.Disparar(direccionDisparo, transform);
            }
        }
    }

    public override void AnimationEvent_OnEnd()
    {
        //base.AnimationEvent_OnEnd(); // Le avisa al StateMachine que terminó
        StateMachine.CurrentState.AnimationFinishTrigger();
        ResetAttackCooldown(); // Reinicia el timer
        
        // ¡La retirada! Le decimos que se aleje y vuelva al estado de movimiento
        estaRetirandose = true; 
        StateMachine.ChangeState(ChaseState);
    }
}