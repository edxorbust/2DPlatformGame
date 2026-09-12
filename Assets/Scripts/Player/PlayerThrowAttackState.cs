using UnityEngine;

public class PlayerThrowAttackState : BaseState
{
    private PlayerController player;

    public PlayerThrowAttackState(PlayerController playerContext)
    {
        player = playerContext;
    }

    public override void Enter()
    {
        player.ThrowAttackInput = false;
        if (player.IsGrounded())
        {
            player.Rb.linearVelocity = new Vector2(0f, player.Rb.linearVelocity.y);
        }
        player.Animator.Play("Throwing");
    }

    public override void AnimationImpactTrigger()
    {
        if (player.shurikenPrefab != null)
        {
            GameObject newShuriken = GameObject.Instantiate(player.shurikenPrefab, player.attackPoint.position, Quaternion.identity);

            newShuriken.GetComponent<Shuriken>().StartShuriken(player);
        }
        player.throwableWeaponCounter--;
    }

    public override void AnimationFinishTrigger()
    {
        player.StateMachine.ChangeState(player.IdleState);
    }
    
}
