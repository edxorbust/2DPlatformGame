using UnityEngine;

public class PlayerHeavyAttackState : BaseState
{
    private PlayerController player;

    // --- VARIABLES DEL SALTO CONTROLADO ---
    /*private int faseSalto = 0; // 0: Windup, 1: Hacia el objetivo, 2: Regresando
    private Vector2 puntoInicial;
    private Vector2 puntoObjetivo;
    private float gravedadOriginal;

    [Header("Ajustes del Ataque")]
    public float velocidadSalto = 25f; // ¡Velocidad muy rápida y definida!
    public Vector2 desplazamiento = new Vector2(0f, 1f); // Cuánto avanza en X y cuánto sube en Y */

    public PlayerHeavyAttackState(PlayerController playerContext)
    {
        player = playerContext;
    }

    public override void Enter()
    {
        //faseSalto = 0; // Reiniciamos la fase
        // 1. Congelamos al jugador y guardamos su gravedad normal
        player.Rb.linearVelocity = Vector2.zero;
        //gravedadOriginal = player.Rb.gravityScale;
       
        // 2. Apagamos la gravedad para tener control total del vuelo
        //player.Rb.gravityScale = 0f;
        Debug.Log($"Entering Heavy Attack - Combo Step: {player.comboStep}");
        player.Animator.Play("HeavyAttack");
        
        // Consumimos el input
        player.HeavyAttackInput = false;
        player.LightAttackInput = false;
    }


    public override void AnimationImpactTrigger()
    {
        // 1. LANZAR EL RADAR DE DAÑO
        player.ReproducirSonido(player.sonidoEspadazo);
        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(player.heavyAttackPoint.position, player.heavyAttackSize, 0f, player.enemyLayers);

        // Interruptor para saber si conectamos el golpe
        bool connectedHit = false;

        // 2. APLICAR DAÑO A CADA ENEMIGO ALCANZADO
        foreach (Collider2D enemy in hitEnemies)
        {
            // Buscamos si es un enemigo con cerebro (Base)
            EnemyController enemyController = enemy.GetComponent<EnemyController>();
            
            if (enemyController != null)
            {
                
                enemyController.GetAttack(player.heavyAttackDamage, player.transform);
                connectedHit = true;
            }
            else
            {
                // Si no tiene cerebro (ej. una caja rompible), solo le hacemos daño normal
                BaseEntity entity = enemy.GetComponent<BaseEntity>();
                if (entity != null)
                {
                    entity.Damage(player.heavyAttackDamage);
                    connectedHit = true;
                }
            }
        }
        
        if (connectedHit)
        {
            player.TriggerHitStop(player.lightHitStopDuration);
        }
    }

    public override void Update()
    {

    }

    public override void AnimationFinishTrigger()
    {
        
        player.StateMachine.ChangeState(player.IdleState);
    }

    public override void WindupFinishedTrigger()
    {
        /* if (faseSalto == 0)
        {
            // PRIMER TRIGGER: Empieza a saltar hacia el objetivo
            faseSalto = 1;
            puntoInicial = player.Rb.position; // Guardamos dónde estamos parados
            
            
            puntoObjetivo = puntoInicial + new Vector2(0f, desplazamiento.y);
        }
        else if (faseSalto == 1)
        {
            // SEGUNDO TRIGGER: Empieza a caer/regresar a la posición original
            faseSalto = 2;
        } */
    }


    public override void PhysicsUpdate()
    {
        float inerciaPlataformaX = 0f;
        if (player.PlataformaActual != null)
        {
            inerciaPlataformaX = player.PlataformaActual.VelocidadActual.x;
        }

        // 3. Aplicamos ambas velocidades sumadas, respetando la gravedad en Y
        player.Rb.linearVelocity = new Vector2(inerciaPlataformaX, player.Rb.linearVelocity.y);
    }

    public override void Exit()
    {
        //faseSalto = 0;
        
        // 3. ¡MUY IMPORTANTE! Devolvemos la gravedad a la normalidad al terminar el estado
        //player.Rb.gravityScale = gravedadOriginal;
        
        // Aseguramos que no se quede con inercia residual
        //player.Rb.linearVelocity = new Vector2(0f, 0f);
    }
}