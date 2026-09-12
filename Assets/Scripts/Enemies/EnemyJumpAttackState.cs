using UnityEngine;

public class EnemyJumpAttackState : BaseState
{
    private SwordsmanController enemy;
    private float jumpForce = 7f; // Ajusta este valor en tu inspector/controlador
    private float dashSpeed = 18f; // Qué tan rápido vuela hacia el jugador
    private float stopDistance = 0.5f;

    // Variables nuevas que necesitas
    private Vector2 posicionGuardada;
    private bool animacionDashIniciada;
    private bool estaHaciendoDash;
    private float gravedadOriginal;

    public EnemyJumpAttackState(SwordsmanController enemyContext)
    {
        enemy = enemyContext;
    }

    public override void Enter()
    {
       // Reiniciamos las variables al entrar al estado
        animacionDashIniciada = false;
        estaHaciendoDash = false;
        
        // Guardamos la gravedad original para restaurarla después
        gravedadOriginal = enemy.Rb.gravityScale;

        enemy.unParryableAttacking = true;
        enemy.cannotBeInterrupted = true;

        // Frena cualquier movimiento horizontal y aplica el salto (simplificado)
        enemy.Rb.linearVelocity = new Vector2(0, jumpForce);
        enemy.Animator.Play("Jump");
    }

    public override void Update()
    {
        // Verificar cuando llega a su punto máximo de altura y asegurarnos de solo llamarlo una vez
        if (!animacionDashIniciada && enemy.Rb.linearVelocity.y <= 0)
        {
            //enemy.FacePlayer();
            Collider2D playerRadar = Physics2D.OverlapCircle(enemy.Rb.position, enemy.detectionRange, enemy.playerLayer);
            animacionDashIniciada = true;
            
            // CONGELAMOS AL ENEMIGO EN EL AIRE (opcional, pero da un efecto dramático muy bueno)
            enemy.Rb.linearVelocity = Vector2.zero;
            enemy.Rb.gravityScale = 0f;

            // GUARDAMOS LA POSICIÓN DEL JUGADOR AQUÍ
            // (Reemplaza 'enemy.playerTransform' con la variable que uses para referirte al jugador)
            if (playerRadar != null)
            {
                // 3. Guardamos la posición del centro exacto del collider
                posicionGuardada = playerRadar.bounds.center;
            }
            else
            {
                // Opcional: ¿Qué pasa si el jugador ya no está en rango?
                // Podemos apuntar hacia adelante a ciegas, o cancelar el ataque.
                // Ejemplo: ataca hacia donde está mirando
                enemy.Rb.gravityScale = gravedadOriginal;
                enemy.StateMachine.ChangeState(enemy.FallState);
                return;
            }
            
            // Damos play a la animación de windup/dash attack
            if (enemy.undodgableAttackSign != null)
            {
                GameObject.Instantiate(enemy.undodgableAttackSign, enemy.warningsPosition.position , Quaternion.identity);
            }
            enemy.Animator.Play("AirDashAttack");
        }
    }

    public override void WindupFinishedTrigger()
    {
        // Le damos permiso a PhysicsUpdate para que empiece a moverlo
        estaHaciendoDash = true;
        //enemy.FacePlayer();
        enemy.ReproducirSonido(enemy.sonidoAtaque);
    }

    public void OnAnimationImpactFrame()
    {
        bool connectedHit = false;
        

        Collider2D[] hits = Physics2D.OverlapCircleAll(enemy.attackPoint.position, enemy.attackDetectorRange, enemy.playerLayer);
        
        foreach (Collider2D hit in hits)
        {
            PlayerController playerController = hit.GetComponent<PlayerController>();

            if (playerController != null)
            {
                enemy.ReproducirSonido(enemy.fleshCut);
                if (enemy.hitSparkPrefab != null)
                {
                    GameObject.Instantiate(enemy.hitSparkPrefab, enemy.attackPoint.position, Quaternion.identity);
                }
                if (CameraController.Instancia != null)
                {
                    CameraController.Instancia.AgitarCamara(0.1f, 0.04f);
                }
                
                playerController.GetAttack(enemy.attackDamage, enemy.transform);
                connectedHit = true;
            }
            else
            {
                BaseEntity entity = hit.GetComponent<BaseEntity>();
                if (entity != null)
                {
                    entity.Damage(enemy.attackDamage);
                }
            }
            if (connectedHit)
            {
                enemy.TriggerHitStop(enemy.lightHitStopDuration);
            }
        }
    }

    public override void PhysicsUpdate()
    {
        if (estaHaciendoDash)
        {
            // 1. Movemos físicamente al enemigo hacia el punto guardado
            Vector2 nuevaPosicion = Vector2.MoveTowards(
                enemy.Rb.position, 
                posicionGuardada, 
                dashSpeed * Time.fixedDeltaTime
            );
            
            enemy.Rb.MovePosition(nuevaPosicion);

            // 2. Verificamos si ya llegó al punto
            float distanciaRestante = Vector2.Distance(enemy.Rb.position, posicionGuardada);
            if (distanciaRestante <= stopDistance)
            {
                // Frenamos el impacto
                estaHaciendoDash = false;
                enemy.Rb.linearVelocity = Vector2.zero;

                bool connectedHit = false;
        

                Collider2D[] hits = Physics2D.OverlapCircleAll(enemy.attackPoint.position, enemy.attackRange, enemy.playerLayer);
                
                foreach (Collider2D hit in hits)
                {
                    PlayerController playerController = hit.GetComponent<PlayerController>();

                    if (playerController != null)
                    {
                        enemy.ReproducirSonido(enemy.fleshCut);
                        if (enemy.hitSparkPrefab != null)
                        {
                            GameObject.Instantiate(enemy.hitSparkPrefab, enemy.attackPoint.position, Quaternion.identity);
                        }
                        if (CameraController.Instancia != null)
                        {
                            CameraController.Instancia.AgitarCamara(0.1f, 0.04f);
                        }
                        
                        playerController.GetAttack(enemy.attackDamage, enemy.transform);
                        connectedHit = true;
                    }
                    else
                    {
                        BaseEntity entity = hit.GetComponent<BaseEntity>();
                        if (entity != null)
                        {
                            entity.Damage(enemy.attackDamage);
                        }
                    }
                    if (connectedHit)
                    {
                        enemy.TriggerHitStop(enemy.lightHitStopDuration);
                    }
                }
                if (!enemy.IsGrounded())
                {
                    enemy.Rb.gravityScale = gravedadOriginal;
                    enemy.StateMachine.ChangeState(enemy.FallState);
                    return;
                }
                // Aquí puedes llamar a terminar el estado o pasar al siguiente movimiento
                enemy.EjecutarSiguienteMovimiento();
            }
        }
    }

    public override void Exit()
    {
        // MUY IMPORTANTE: Devolverle la gravedad al enemigo al salir del estado
        enemy.Rb.gravityScale = gravedadOriginal;
        estaHaciendoDash = false;
        enemy.unParryableAttacking = false;
        enemy.cannotBeInterrupted = false;
    }

}
