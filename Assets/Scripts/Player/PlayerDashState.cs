using UnityEngine;

public class PlayerDashState : BaseState
{
    private PlayerController player;
    private float dashTimer;
    private float originalGravity; // Para guardar la gravedad original

    public PlayerDashState(PlayerController playerContext)
    {
        player = playerContext;
    }

    public override void Enter()
    {
        Debug.Log("Entering Dash State");
        
        // Consumimos el input y marcamos el tiempo en el que se hizo el dash
        player.inmuneToAttacks = true;
        player.DashInput = false;
        player.LastDashTime = Time.time;
        dashTimer = player.dashDuration;

        // 1. Guardamos la gravedad original y la apagamos temporalmente
        originalGravity = player.Rb.gravityScale;
        player.Rb.gravityScale = 0f;

        // 2. Aplicamos la velocidad masiva hacia donde esté mirando el jugador
        // Si IsFacingRight es true, la dirección es 1. Si es false, es -1.
        float dashDirection = player.IsFacingRight ? 1f : -1f;
        player.Rb.linearVelocity = new Vector2((player.dashSpeed * dashDirection) + player.Rb.linearVelocity.x, player.Rb.linearVelocity.y);

        // 3. I-FRAMES: Reutilizamos tu bandera de invulnerabilidad del Parry
        player.Health.IsDashing = true; 
        player.Animator.Play("Dash");
    }

    public override void Update()
    {
        dashTimer -= Time.deltaTime;

        if (dashTimer <= 0f)
        {
            // Verificamos a qué estado volver dependiendo de si hay suelo
            if (player.IsGrounded())
            {
                player.StateMachine.ChangeState(player.IdleState);
            }
            else
            {
                player.StateMachine.ChangeState(player.FallState);
            }
        }
    }

    public override void PhysicsUpdate()
    {
        // No permitimos cambiar de dirección, el jugador va en línea recta
    }

    public override void Exit()
    {
        // IMPORTANTE: Restauramos la gravedad, apagamos los I-frames y frenamos al jugador
        player.inmuneToAttacks = false;
        player.Rb.gravityScale = originalGravity;
        player.Health.IsDashing = false;
        player.Rb.linearVelocity = new Vector2(0f, player.Rb.linearVelocity.y);
        
        Debug.Log("Exiting Dash State");
    }
}