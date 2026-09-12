using UnityEngine;

public class PlayerWallJumpState : BaseState
{
    private PlayerController player;
    
    // --- MAGIA AAA: El Bloqueo de Input ---
    private float lockoutTimer;
    public float lockoutTime = 0.2f; 
    private float jumpDirection; 

    // --- MAGIA AAA: Variables del Jump Cut (Salto Variable) ---
    private float jumpCutMultiplier = 0.1f; 
    private bool jumpCutApplied;

    public PlayerWallJumpState(PlayerController playerContext)
    {
        player = playerContext;
    }

    public override void Enter()
    {
        player.CanFlip = false;
        player.Animator.Play("WallJump");

        // Reiniciamos la bandera del corte de salto
        jumpCutApplied = false;

        player.JumpReleaseInput = false;

        // Determinamos y guardamos la dirección del salto
        jumpDirection = player.IsFacingRight ? -1f : 1f;
        player.Flip();

        player.Rb.linearVelocity = new Vector2(jumpDirection * player.wallJumpForce.x, player.wallJumpForce.y);
        
        // Iniciamos el bloqueo
        lockoutTimer = lockoutTime;
    }

    public override void Update()
    {
        // Descontamos el tiempo del bloqueo
        if (lockoutTimer > 0f)
        {
            lockoutTimer -= Time.deltaTime;
        }

        // --- LA MAGIA DEL SALTO VARIABLE ---
        // Si el jugador suelta el botón, no se ha cortado aún, y sigue subiendo...
        // Si el jugador suelta el botón, no se ha cortado aún, y sigue subiendo...
        if (player.JumpReleaseInput && !jumpCutApplied && player.Rb.linearVelocity.y > 0f)
        {
            // 1. Cortamos su velocidad vertical a la mitad (o el multiplicador que elijas)
            float newVelY = player.Rb.linearVelocity.y * jumpCutMultiplier;
            
            // 2. ¡EL SECRETO! Cortamos también su inercia horizontal hacia afuera
            float newVelX = player.Rb.linearVelocity.x * jumpCutMultiplier;
            
            player.Rb.linearVelocity = new Vector2(newVelX, newVelY);

            // 3. ¡CANCELAMOS EL BLOQUEO! 
            // Esto le devuelve el control del joystick al jugador de inmediato para que pueda dirigirse de vuelta a la pared.
            lockoutTimer = 0f;

            jumpCutApplied = true;
            player.JumpReleaseInput = false; // Consumimos el input
        }

        // Solo transitamos al estado de caída si ya terminó el bloqueo
        if (player.Rb.linearVelocity.y < 0f && lockoutTimer <= 0f)
        {
            player.StateMachine.ChangeState(player.FallState);
            return;
        }

        if (player.LightAttackInput)
        {
            player.StateMachine.ChangeState(player.LightAttackState);
            return; 
        }
        
        if (player.IsGrounded() && player.Rb.linearVelocity.y <= 0f)
        {
            player.StateMachine.ChangeState(player.IdleState);
            return;
        }
    }

    public override void PhysicsUpdate()
    {
        // Durante el tiempo de bloqueo, sobrescribimos la velocidad horizontal constantemente.
        // Esto evita que cualquier otro script (o el input del jugador) frene el salto en el aire.
        if (lockoutTimer > 0f)
        {
            player.Rb.linearVelocity = new Vector2(jumpDirection * player.wallJumpForce.x, player.Rb.linearVelocity.y);
        }
    }

    public override void Exit()
    {
        player.CanFlip = true;
        
        // Limpiamos el input por si acaso soltó el botón al cambiar de estado
        player.JumpReleaseInput = false;
    }
}