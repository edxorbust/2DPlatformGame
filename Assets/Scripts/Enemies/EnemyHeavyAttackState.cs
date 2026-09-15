using UnityEngine;

public class EnemyHeavyAttackState : BaseState
{
    private EnemyController enemy;

    public EnemyHeavyAttackState(EnemyController enemyContext)
    {
        enemy = enemyContext;
    }

    public override void Enter()
    {
        enemy.cannotBeInterrupted = true;
        enemy.unBlockableAttacking = true;
        if (enemy.Health.InSecondPhase)
        {
            enemy.Animator.Play("HeavyAttack2");
        }
        else
        {
            enemy.Animator.Play("HeavyAttack");
        }
    }

    public override void Update() { }

    public void OnAnimationImpactFrame()
    {
        bool connectedHit = false;
        

        Collider2D[] hits = Physics2D.OverlapCircleAll(enemy.attackPoint.position, enemy.attackDetectorRange, enemy.playerLayer);
        
        foreach (Collider2D hit in hits)
        {
            PlayerController playerController = hit.GetComponent<PlayerController>();
            enemy.ReproducirSonido(enemy.sonidoAtaque); 

            if (playerController != null)
            {
                if (!playerController.Health.IsParrying)
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
                    playerController.pushedUp = true;
                    playerController.hurtAndPushDirection = (playerController.transform.position.x > enemy.transform.position.x) ? 1f : -1f;
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
        if (!connectedHit)
        {
            enemy.ReproducirSonido(enemy.sonidoAtaque);
        }
    }

    

    public void OnAnimationEndFrame()
    {
        if(enemy.CanSeePlayer())
        {
            
            enemy.EjecutarSiguienteMovimiento();
        }
        else
        {
            enemy.StateMachine.ChangeState(enemy.IdleState);
        }
    }

    public override void PhysicsUpdate()
    {
        float inerciaPlataformaX = 0f;
        if (enemy.PlataformaActual != null)
        {
            inerciaPlataformaX = enemy.PlataformaActual.VelocidadActual.x;
        }

        // 3. Aplicamos ambas velocidades sumadas, respetando la gravedad en Y
        enemy.Rb.linearVelocity = new Vector2(inerciaPlataformaX, enemy.Rb.linearVelocity.y);
    }

    public override void Exit()
    {
        Debug.Log("Saliendo de attack");
        enemy.ResetAttackCooldown();
        enemy.cannotBeInterrupted = false;
        enemy.unBlockableAttacking = false;
    }
}