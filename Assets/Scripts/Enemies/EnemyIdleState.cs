using UnityEngine;

public class EnemyIdleState : BaseState
{
    private EnemyController enemy;
    private float timer;

    public EnemyIdleState(EnemyController enemyContext)
    {
        enemy = enemyContext;
    }

    public override void Enter()
    {
        Debug.Log("Enemigo: Entrando a Idle");
        if (enemy.Health.InSecondPhase)
        {
            enemy.Animator.Play("Idle2");
        }
        else
        {
            enemy.Animator.Play("Idle");
        }
       
        enemy.Rb.linearVelocity = new Vector2(0f, enemy.Rb.linearVelocity.y);
        timer = enemy.idleTime;
    }

    public override void Update()
    {
        // 1. Lógica del Enemigo Volador (Se mantiene igual)
        if (enemy.isFlyingEnemy)
        {
            if (enemy.DistanceToPlayer() <= enemy.detectionRange)
            {
                enemy.StateMachine.ChangeState(enemy.ChaseState);
            }
            return; // Cortamos aquí para que no siga ejecutando código terrestre
        }

        if (!enemy.IsGrounded() && !enemy.isFlyingEnemy)
        {
            enemy.StateMachine.ChangeState(enemy.FallState);
        }

        // 2. Temporizador de Patrulla para Enemigos Terrestres
        timer -= Time.deltaTime;
        if (timer <= 0f && !enemy.Health.isBoss && !enemy.CanSeePlayer() && enemy.IsGrounded())
        {
            enemy.StateMachine.ChangeState(enemy.PatrolState); 
        }

        // 3. Detección del Jugador
        if (enemy.CanSeePlayer() && enemy.IsGrounded())
        {
            if (enemy.isRangeEnemy) 
            {
                ArcherController archer = enemy as ArcherController;
                if (archer != null)
                {
                    enemy.StateMachine.ChangeState(archer.EscapeState);
                }
            }
            else 
            {
                // Lógica original conservada para el Espadachín
                if (enemy.DistanceToPlayer() <= enemy.attackRange && enemy.CanAttack())
                {
                    enemy.IniciarAtaque();
                    return;
                }
                else if(enemy.DistanceToPlayer() > enemy.longAttackRange || enemy.DistanceToPlayer() > enemy.attackRange) 
                {
                    if (enemy.IsThereGroundNext())
                    {
                        enemy.StateMachine.ChangeState(enemy.ChaseState);
                    }
                    else
                    {
                        enemy.FacePlayer();
                        if (enemy.IsThereGroundNext())
                        {
                            enemy.StateMachine.ChangeState(enemy.ChaseState);
                        }
                    }
                }
            }
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
    public override void Exit() { }
}