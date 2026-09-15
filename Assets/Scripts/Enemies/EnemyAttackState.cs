using UnityEngine;

public class EnemyAttackState : BaseState
{
    private EnemyController enemy;
    private int comboCounter;
    private bool continueAttacking;

    public EnemyAttackState(EnemyController enemyContext)
    {
        enemy = enemyContext;
    }

    public override void Enter()
    {
        comboCounter = 1;
        enemy.cannotBeInterrupted = true;
        if (enemy.isFlyingEnemy)
        {
            enemy.IniciarAtaque();
        }
        Attacking();
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
                if (!playerController.Health.IsBlocking && !playerController.Health.IsParrying)
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

    private void Attacking()
    {
        enemy.Rb.linearVelocity = new Vector2(0f, enemy.Rb.linearVelocity.y);
        // AQUI ESTABA EL ERROR: Cambiado a sonidoAtaque
        continueAttacking = enemy.GetRandomBoolean();
        
        if (comboCounter == 1)
        {
            if (enemy.Health.InSecondPhase)
            {
                enemy.Animator.Play("LightAttack12");
            }
            else
            {
                enemy.Animator.Play("LightAttack1");
            }
        }
        else if (comboCounter == 2)
        {
            if (enemy.Health.InSecondPhase)
            {
                enemy.Animator.Play("LightAttack22");
            }
            else
            {
                enemy.Animator.Play("LightAttack2");
            }
        }
        else if (comboCounter >= 3)
        {
            if (enemy.Health.InSecondPhase)
            {
                enemy.Animator.Play("LightAttack32");
            }
            else
            {
                enemy.Animator.Play("LightAttack3");
            }
        }

        if (!continueAttacking)
        {
            comboCounter = 3;
        }
    }

    public void OnAnimationEndFrame()
    {
        if(enemy.CanSeePlayer())
        {
            if (comboCounter < 3)
            {
                comboCounter++;
                Attacking();
            }
            else
            {
                enemy.EjecutarSiguienteMovimiento();
            }
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
        comboCounter = 0;
        enemy.ResetAttackCooldown();
        enemy.cannotBeInterrupted = false;
    }
}