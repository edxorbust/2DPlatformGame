using UnityEngine;

public class PlayerJumpState : BaseState
{
    private PlayerController player;

    // --- MAGIA AAA: Variables del Jump Cut ---
    private float jumpCutMultiplier = 0.5f; 
    private bool jumpCutApplied;

    public PlayerJumpState(PlayerController playerContext)
    {
        player = playerContext;
    }

    public override void Enter()
    {
        // Vaciamos el reloj para que no pueda volver a saltar mágicamente en el aire
        player.coyoteTimer = 0f; 
        
        // Reiniciamos el corte al iniciar el salto para que pueda volver a usarse
        jumpCutApplied = false;

        player.JumpReleaseInput = false;

        Debug.Log("Entering Jump State"); 
        
        // Aplicamos el impulso vertical en Y, manteniendo la velocidad horizontal que ya llevaba
        player.Rb.linearVelocity = new Vector2(player.Rb.linearVelocity.x, player.jumpForce); 
        
        // ¡CRÍTICO! Consumimos el input poniendo la bandera en false. 
        player.JumpInput = false; 
        player.Animator.Play("Jump"); 
    }

    public override void Update()
    {
        // --- LA MAGIA DEL SALTO VARIABLE ---
        // Si el jugador suelta el botón de salto, no lo hemos cortado aún, y sigue subiendo...
        if (player.JumpReleaseInput && !jumpCutApplied && player.Rb.linearVelocity.y > 0f)
        {
            // Cortamos su velocidad vertical a la mitad
            player.Rb.linearVelocity = new Vector2(player.Rb.linearVelocity.x, player.Rb.linearVelocity.y * jumpCutMultiplier);
            jumpCutApplied = true;
            player.JumpReleaseInput = false; // Consumimos el input de soltar
        }

        // CONTROL DE PARED
        if (player.IsTouchingWall()) 
        {
            bool pushingRight = player.MovementInput.x > 0.1f && player.IsFacingRight; 
            bool pushingLeft = player.MovementInput.x < -0.1f && !player.IsFacingRight; 

            if (pushingRight || pushingLeft) 
            {
                player.StateMachine.ChangeState(player.WallSlideState); 
                return; // Retornamos para que no ejecute el resto del Update
            }
        }

        // ATAQUES EN EL AIRE
        if (player.LightAttackInput) 
        {
            player.StateMachine.ChangeState(player.LightAttackState); 
            return; 
        }

        if (player.ThrowAttackInput) 
        {
            player.StateMachine.ChangeState(player.ThrowAttackState); 
            return;
        }

        // ATERRIZAJE
        if (player.Rb.linearVelocity.y <= 0.1f && player.IsGrounded()) 
        {
            // Decidimos a qué estado volver según si se está moviendo o no
            if (Mathf.Abs(player.MovementInput.x) > 0.1f) 
            {
                player.StateMachine.ChangeState(player.MoveState); 
            }
            else 
            {
                player.StateMachine.ChangeState(player.IdleState); 
            }
            return; // ¡Importante cortar el flujo si ya cambiamos de estado!
        }

        // CAÍDA NORMAL (Si llegó al ápice del salto y empieza a bajar)
        if (player.Rb.linearVelocity.y <= 0f) 
        {
            player.StateMachine.ChangeState(player.FallState); 
            return;
        }

        // CONTROL DE DASH
        // Verificamos si se presionó el botón Y si ya pasó el tiempo de recarga
        if (player.DashInput && Time.time >= player.LastDashTime + player.dashCooldown) 
        {
            player.StateMachine.ChangeState(player.DashState); 
            return;
        }
        // Si el jugador presionó el botón pero el cooldown no ha terminado, consumimos el input
        else if (player.DashInput) 
        {
            player.DashInput = false; 
        }
    }

    public override void PhysicsUpdate()
    {
        // Movimiento aéreo: Permitimos que el jugador se mueva un poco en el aire usando A/D mientras salta
        player.Rb.linearVelocity = new Vector2(player.MovementInput.x * player.moveSpeed, player.Rb.linearVelocity.y); 
    }

    public override void Exit()
    {
        Debug.Log("Exiting Jump State"); 
        player.Animator.Play("TransitionJump"); 
        
        // Limpiamos el input por si acaso el jugador presionó y soltó en un estado donde no le tocaba saltar
        player.JumpReleaseInput = false;
    }
}