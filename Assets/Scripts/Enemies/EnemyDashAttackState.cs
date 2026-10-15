using UnityEngine;

public class EnemyDashAttackState : BaseState
{
    private SwordsmanController enemy;
    private float dashTimer;
    private float dashDuration = 1f; // Cuánto dura el impulso
    private bool damageGiven;

    public EnemyDashAttackState(SwordsmanController enemyContext)
    {
        enemy = enemyContext;
    }

    public override void Enter()
    {
        // 1. Iniciamos la animación
        enemy.Rb.linearVelocity = new Vector2(0, enemy.Rb.linearVelocity.y);
        enemy.unBlockableAttacking = true;
        damageGiven = false;
        enemy.cannotBeInterrupted = true;
        enemy.touchDamageActivated = false;
        enemy.Animator.Play("DashAttack");
        if (enemy.undodgableAttackSign != null)
        {
            GameObject.Instantiate(enemy.undodgableAttackSign, enemy.warningsPosition.position , Quaternion.identity);
        }

        

        // 3. Reiniciamos el temporizador
        dashTimer = 0f;
    }

    public override void WindupFinishedTrigger()
    {
        // 2. Aplicamos la fuerza física hacia donde esté mirando el enemigo
        // Asumiendo que usas transform.right o tienes una variable de dirección
        enemy.ReproducirSonido(enemy.sonidoAtaque);
        float direccion = Mathf.Sign(enemy.transform.localScale.x); // 1 (derecha) o -1 (izquierda)
        enemy.Rb.linearVelocity = new Vector2(direccion * enemy.dashForce, enemy.Rb.linearVelocity.y);
    }

    public void OnAnimationImpactFrame()
    {
        bool connectedHit = false;
        if (damageGiven)
        {
            return;
        }
        

        Collider2D[] hits = Physics2D.OverlapCircleAll(enemy.attackPoint.position, enemy.attackDetectorRange, enemy.playerLayer);
        
        foreach (Collider2D hit in hits)
        {
            PlayerController playerController = hit.GetComponent<PlayerController>();

            if (playerController != null)
            {
                if (!playerController.Health.IsParrying)
                {
                    enemy.ReproducirSonido(enemy.fleshCut);
                    if (enemy.hitSparkPrefab != null)
                    {
                        GameObject.Instantiate(enemy.hitSparkPrefab, enemy.attackPoint.position, Quaternion.identity);
                    }
                    
                }
                playerController.GetAttack(enemy.attackDamage, enemy.transform);
                damageGiven = true;
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

    public override void Update()
    {
        dashTimer += Time.deltaTime;

        // Esperamos a que termine el tiempo del Dash (o puedes usar Animation Events)
        if (dashTimer >= dashDuration)
        {
            // ¡EL TRUCO DE LA SECUENCIA!
            // En lugar de volver a Idle, le decimos al Controller que ejecute el siguiente movimiento
            enemy.EjecutarSiguienteMovimiento(); 
        }

        if (!enemy.IsThereGroundNext())
        {
            enemy.Rb.linearVelocity = new Vector2(0, enemy.Rb.linearVelocity.y);
        }
    }

    public override void Exit()
    {
        // Frenamos al enemigo al salir del estado para que no resbale
        enemy.Rb.linearVelocity = new Vector2(0, enemy.Rb.linearVelocity.y);
        enemy.unBlockableAttacking = false;
        enemy.isAttackingFar = false;
        enemy.cannotBeInterrupted = false;
        enemy.touchDamageActivated = true;
        damageGiven = false;
    }
}
