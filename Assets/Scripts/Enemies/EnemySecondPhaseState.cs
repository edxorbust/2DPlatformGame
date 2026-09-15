using UnityEngine;

public class EnemySecondPhaseState : BaseState
{
    private EnemyController enemy;
    

    public EnemySecondPhaseState(EnemyController enemyContext)
    {
        enemy = enemyContext;
    }

    public override void Enter()
    {
        Debug.Log("entering second phase");
        enemy.inmuneToAttacks = true;
        enemy.Animator.Play("SecondPhase");
    }

    public override void WindupFinishedTrigger()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(enemy.transform.position, enemy.detectionRange, enemy.playerLayer);
        foreach (Collider2D hit in hits)
        {
            PlayerController player = hit.GetComponent<PlayerController>();
            player.beingPushed = true;
            player.hurtAndPushDirection = (player.transform.position.x > enemy.transform.position.x) ? 1f : -1f;
            player.StateMachine.ChangeState(player.HurtState);
        }
    }


    public override void PhysicsUpdate()
    {
    }

    public override void AnimationFinishTrigger()
    {
        enemy.inmuneToAttacks = false;
        enemy.StateMachine.ChangeState(enemy.IdleState); 
    }
}
