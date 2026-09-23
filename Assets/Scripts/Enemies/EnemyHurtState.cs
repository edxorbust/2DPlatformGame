using UnityEngine;

public class EnemyHurtState : BaseState
{
    private EnemyController enemy;
    private float maxTimeHurt = 0.5f;
    private float timer;
    

    public EnemyHurtState(EnemyController enemyContext)
    {
        enemy = enemyContext;
    }

    public override void Enter()
    {
        timer = maxTimeHurt;
        enemy.inmuneToAttacks = true;
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
        timer -= Time.deltaTime;
        if (timer <= 0 && enemy.StateMachine.CurrentState == enemy.HurtState)
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

    public override void AnimationFinishTrigger()
    {
        // Cuando la animación de Hurt termine, volvemos a Idle y le devolvemos el control
        enemy.StateMachine.ChangeState(enemy.IdleState);
    }

    public override void Exit()
    {
        enemy.inmuneToAttacks = false;
    }
}
