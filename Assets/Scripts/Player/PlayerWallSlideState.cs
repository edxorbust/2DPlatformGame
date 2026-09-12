using UnityEngine;

public class PlayerWallSlideState : BaseState
{
    private PlayerController player;
    
    // --- MAGIA AAA: El temporizador de pegamento ---
    private float unstickTimer; 
    public float unstickTime = 0.15f; // Tiempo de gracia antes de soltarse

    public PlayerWallSlideState(PlayerController playerContext)
    {
        player = playerContext;
    }

    public override void Enter()
    {
        player.CanFlip = false;
        
        if (player.spriteRenderer != null)
        {
            player.spriteRenderer.flipX = true; 
        }
        
        player.Rb.linearVelocity = new Vector2(0f, 0f);
        player.Animator.Play("WallContact");
        
        // Reiniciamos el temporizador al tocar la pared
        unstickTimer = unstickTime;
    }

    public override void Update()
    {
        if (player.JumpInput)
        {
            player.JumpInput = false; 
            player.StateMachine.ChangeState(player.WallJumpState);
            return;
        }

        if (player.IsGrounded())
        {
            player.StateMachine.ChangeState(player.IdleState);
            return;
        }

        float xInput = player.MovementInput.x;
        bool isMovingAway = (player.IsFacingRight && xInput < 0) || (!player.IsFacingRight && xInput > 0);
        
        if (!player.IsTouchingWall())
        {
            player.StateMachine.ChangeState(player.FallState);
            return;
        }

        // --- LÓGICA DEL PEGAMENTO ---
        if (isMovingAway)
        {
            // Si intenta alejarse, empezamos a descontar el tiempo
            unstickTimer -= Time.deltaTime;
            
            // Solo lo soltamos si se acaba el tiempo de gracia
            if (unstickTimer <= 0f)
            {
                player.StateMachine.ChangeState(player.FallState);
                return;
            }
        }
        else
        {
            // Si deja de alejarse o vuelve a presionar hacia la pared, recupera el agarre
            unstickTimer = unstickTime; 
        }

        if (player.Rb.linearVelocity.y < 0)
        {
            player.Animator.Play("SlidingWall");
        }
    }

    public override void PhysicsUpdate()
    {
        player.Rb.linearVelocity = new Vector2(0f, -player.wallSlideSpeed);
    }

    public override void Exit()
    {
        player.CanFlip = true;
        if (player.spriteRenderer != null)
        {
            player.spriteRenderer.flipX = false;
        }
    }
}