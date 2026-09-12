using UnityEngine;

public class EnemyFallState : BaseState
{
    private EnemyController enemy;

    public EnemyFallState(EnemyController enemyContext)
    {
        enemy = enemyContext;
    }

    public override void Enter()
    {
        enemy.Animator.Play("Fall");
    }

    public override void Update()
    {
        if (enemy.IsGrounded())
        {
            enemy.StateMachine.ChangeState(enemy.IdleState);
        }
    }
}
