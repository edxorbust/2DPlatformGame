using UnityEngine;

public class EnemyPatrolState : BaseState
{
    private EnemyController enemy;
    private float timer;

    public EnemyPatrolState(EnemyController enemyContext)
    {
        enemy = enemyContext;
    }

    public override void Enter()
    {
        enemy.Animator.Play("Run"); // O la animación de caminar que tengas
        timer = enemy.patrolTime;
    }

    public override void Update()
    {
        // 1. Si el jugador entra en rango, cancelamos la patrulla y atacamos
        if (enemy.CanSeePlayer() && !enemy.isRangeEnemy)
        {
            enemy.StateMachine.ChangeState(enemy.ChaseState);
            return;
        }

        // 2. Temporizador de patrulla
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            enemy.StateMachine.ChangeState(enemy.IdleState);
        }
    }

    public override void PhysicsUpdate()
    {
        // 3. Revisar si hay un precipicio frente a nosotros
        if (!enemy.IsThereGroundNext())
        {
            Girar();
        }

        // 4. Moverse hacia adelante según hacia dónde esté mirando
        float direccionX = (enemy.transform.localScale.x == -1f) ? -1f : 1f;
        enemy.Rb.linearVelocity = new Vector2(direccionX * enemy.patrolSpeed, enemy.Rb.linearVelocity.y);
    }

    public override void Exit()
    {
        // Limpiamos la inercia al salir
        enemy.Rb.linearVelocity = new Vector2(0f, enemy.Rb.linearVelocity.y);
    }

    private void Girar()
    {
        if (enemy.transform.localScale.x == 1f)
            enemy.transform.localScale = new Vector3(-1f, 1f, 0);
        else
            enemy.transform.localScale = new Vector3(1f, 1f, 0);
    }
}