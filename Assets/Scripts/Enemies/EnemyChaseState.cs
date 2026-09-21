using UnityEngine;

public class EnemyChaseState : BaseState
{
    private EnemyController enemy;
    private bool compromisedToMelee = false;

    public EnemyChaseState(EnemyController enemyContext)
    {
        enemy = enemyContext;
    }

    public override void Enter()
    {
        Debug.Log("Enemigo: Entrando a Chase (Perseguir)");
        compromisedToMelee = false;
        if (enemy.Health.InSecondPhase)
        {
            enemy.Animator.Play("Run2");
        }
        else
        {
            enemy.Animator.Play("Run");
        }
        
        enemy.Rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    public override void Update()
    {
        if (!enemy.isRangeEnemy)
        {
            enemy.FacePlayer();
        }

        float distance = enemy.DistanceToPlayer();
        if (!enemy.isFlyingEnemy && !enemy.isRangeEnemy)
        {
            if (!enemy.IsThereGroundNext())
            {
                enemy.StateMachine.ChangeState(enemy.IdleState);
                return;
            }
        }

        if (distance > enemy.detectionRange)
        {
            enemy.StateMachine.ChangeState(enemy.IdleState);
            return;
        }
        
        if (!enemy.isFlyingEnemy && !enemy.isRangeEnemy && enemy.CanSeePlayer())
        {
            if (distance > enemy.longAttackRange)
            {
                compromisedToMelee = false;
            }

            if (distance <= enemy.attackRange)
            {
                compromisedToMelee = false;
                if (enemy.CanAttack())
                {
                    enemy.IniciarAtaque();
                    return;
                }
                else
                {
                    enemy.StateMachine.ChangeState(enemy.IdleState);
                    return;
                }
            }

            if (distance <= enemy.longAttackRange && distance > enemy.attackRange && !compromisedToMelee)
            {
                if (enemy.CanAttack())
                {
                    bool pudoAtacarDeLejos = enemy.IntentarAtaqueLargo();
                    if (!pudoAtacarDeLejos)
                    {
                        // Si la tarjeta decía que tocaba Melee, el enemigo no ataca.
                        // Entonces le decimos: "Comprométete a caminar, no vuelvas a tirar los dados".
                        compromisedToMelee = true;
                    }
                }
                
            }
        }
        
    }

    public override void PhysicsUpdate()
    {
        float moveDirection = enemy.playerTarget.position.x > enemy.transform.position.x ? 1f : -1f;
        if (enemy.PlataformaActual != null)
        {
            float inerciaPlataformaX = enemy.PlataformaActual.VelocidadActual.x;
            enemy.Rb.linearVelocity = new Vector2((moveDirection * enemy.moveSpeed)+inerciaPlataformaX, enemy.Rb.linearVelocity.y);
            return;
        }

        enemy.Rb.linearVelocity = new Vector2(moveDirection * enemy.moveSpeed, enemy.Rb.linearVelocity.y);
    }

    public override void Exit() { }
}