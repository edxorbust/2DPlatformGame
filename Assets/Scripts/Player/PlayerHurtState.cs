using UnityEngine;

public class PlayerHurtState : BaseState // O el nombre que tenga tu clase base
{
    private PlayerController player;
    public float hitDirection;
    public SwordsmanController swordsman;
    private float timeBeingPushed = 0.5f;
    private float maxHurtTime = 0.16f;
    private float timer;

    public PlayerHurtState(PlayerController playerContext)
    {
        player = playerContext;
    }

    public override void Enter()
    {
        
        player.inmuneToAttacks = true;
        hitDirection = player.hurtAndPushDirection;
        if (player.beingPushed)
        {
            Debug.Log("entrando a push");
            player.Animator.Play("Pushed");
            timer = timeBeingPushed;
            if (player.Rb != null)
            {
               

                player.Rb.linearVelocity = new Vector2(hitDirection * 8f, player.Rb.linearVelocity.y);
            }
        }
        else if (player.pushedUp)
        {
            player.Animator.Play("Pushed");
            timer = timeBeingPushed;
            if (player.Rb != null)
            {
                

                player.Rb.linearVelocity = new Vector2(hitDirection * 6f, 8f);
            }
        }
        else
        {
            player.Animator.Play("Hurt");
            timer = maxHurtTime;
            
        }
 
        player.LightAttackInput = false;
        player.HeavyAttackInput = false;
        player.JumpInput = false;
    }

    public override void Update()
    {
        
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            player.StateMachine.ChangeState(player.IdleState);
        }
        
    }


    // public override void AnimationFinishTrigger()
    // {
        
    //     if (player.beingPushed || player.pushedUp)
    //     {
            
    //     }
    //     else
    //     {
    //         player.StateMachine.ChangeState(player.IdleState);
    //     }
    // }

    public override void Exit()
    {
        if (player.beingPushed)
        {
            player.beingPushed = false;
        }
        if (player.pushedUp)
        {
            player.pushedUp = false;
        }
        player.inmuneToAttacks = false;
    }
}