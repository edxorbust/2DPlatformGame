using UnityEngine;

public class EnemyStunnedState : BaseState
{
    private EnemyController enemy;
    private float stunTimer;
    private SpriteRenderer spriteRenderer;

    public EnemyStunnedState(EnemyController enemyContext)
    {
        enemy = enemyContext;
    }

    public override void Enter()
    {
        Debug.Log("Enemigo: ¡POSTURA ROTA! Vulnerable a Finisher.");
        enemy.Animator.Play("Stunned");
        stunTimer = enemy.stunDuration;
        
        enemy.Rb.linearVelocity = new Vector2(0f, enemy.Rb.linearVelocity.y);
        //enemy.Rb.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionX;
    }

    public override void Update()
    {
        stunTimer -= Time.deltaTime;

        if (stunTimer <= 0f)
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
        Debug.Log("Enemigo: Recuperó la postura y continúa la pelea.");
        
        // ¡VITAL! Restauramos la barra de postura al máximo
        if(enemy.Health.finisherLimit > 0)
        {
            enemy.Health.RestartPosture();
        }
    }
}