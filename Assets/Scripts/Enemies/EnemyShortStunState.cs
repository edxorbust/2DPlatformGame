using UnityEngine;

public class EnemyShortStunState : BaseState
{
    private EnemyController enemy;
    private float maxTimeStunned = 1.5f;
    private float timer;
    public EnemyShortStunState(EnemyController enemyContext)
    {
        enemy = enemyContext;
    }

    public override void Enter()
    {
        timer = maxTimeStunned;
        enemy.Animator.Play("ShortStun");
    }

    public override void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            enemy.StateMachine.ChangeState(enemy.IdleState);
        }
    }
}
