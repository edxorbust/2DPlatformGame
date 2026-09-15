using UnityEngine;

public class EnemyHurtState : BaseState
{
    private EnemyController enemy;
    

    public EnemyHurtState(EnemyController enemyContext)
    {
        enemy = enemyContext;
    }

    public override void Enter()
    {

        if (enemy.Health.InSecondPhase)
        {
            enemy.Animator.Play("Hurt2");
        }
        else
        {
            enemy.Animator.Play("Hurt");
        }
        enemy.Rb.linearVelocity = new Vector2(0f, enemy.Rb.linearVelocity.y);
        
    }

    public override void Update()
    {
        
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

    public override void AnimationFinishTrigger()
    {
        // Cuando la animación de Hurt termine, volvemos a Idle y le devolvemos el control
        enemy.StateMachine.ChangeState(enemy.IdleState);
    }
}
