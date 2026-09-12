using UnityEngine;

public class EnemyBlockState : BaseState
{
    private EnemyController enemy;
    private float timeB;

    public EnemyBlockState(EnemyController enemyContext)
    {
        enemy = enemyContext;
    }
    
    public override void Enter()
    {
        enemy.isBlocking = true;
        enemy.Animator.Play("Block");
        timeB = enemy.blockTime;
        
    }

    public override void Update()
    {
        timeB -= Time.deltaTime;

        if (timeB <= 0)
        {
            enemy.StateMachine.ChangeState(enemy.IdleState);
        }
        if (enemy.Health.ActualPosture == 0)
        {
            enemy.StateMachine.ChangeState(enemy.StunnedState);
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
        enemy.isBlocking = false;
    }
}