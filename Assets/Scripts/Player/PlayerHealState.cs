using UnityEngine;

public class PlayerHealState : BaseState
{
    private PlayerController player;

    public PlayerHealState(PlayerController playerContext)
    {
        player = playerContext;
    }

    public override void Enter()
    {
        Debug.Log("Entering Heal State");
        player.HealInput = false;
        player.Health.HealPotions--;
        player.Animator.Play("Heal");
        player.Rb.linearVelocity = new Vector2(0f, player.Rb.linearVelocity.y);
    }

    public override void AnimationFinishTrigger()
    {
        player.Health.Cure(player.Health.cureQuantity);
        player.StateMachine.ChangeState(player.IdleState);
    }
}
