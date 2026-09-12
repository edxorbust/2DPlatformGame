using UnityEngine;

public class PlayerFallState : BaseState
{
    private PlayerController player;

    // Constructor que recibe el "cerebro" del jugador
    public PlayerFallState(PlayerController playerContext)
    {
        player = playerContext;
    }

    public override void Enter()
    {
        Debug.Log("Entering Fall State");
        // Aquí en el futuro llamaremos a la animación: player.Animator.Play("Fall");
        player.Animator.Play("Fall");
    }

    public override void Update()
    {
        // NUEVO: ¿Presionó saltar y todavía le queda Coyote Time?
        if (player.JumpInput && player.coyoteTimer > 0f)
        {
            player.StateMachine.ChangeState(player.JumpState);
            return; // Salimos del Update para no ejecutar la lógica de caída
        }
        if (player.IsTouchingWall())
        {
            bool pushingRight = player.MovementInput.x > 0.1f && player.IsFacingRight;
            bool pushingLeft = player.MovementInput.x < -0.1f && !player.IsFacingRight;

            if (pushingRight || pushingLeft)
            {
                player.StateMachine.ChangeState(player.WallSlideState);
                return; // Retornamos para que no ejecute el resto del FallState
            }
        }

        if (player.LightAttackInput)
        {
            player.StateMachine.ChangeState(player.LightAttackState);
            return; // Usamos return para cortar el Update y no transitar a otro estado a la vez
        }

        if (player.ThrowAttackInput)
        {
            player.StateMachine.ChangeState(player.ThrowAttackState);
            return;
        }

        // CONTROL DE ATERRIZAJE: Revisamos constantemente si tocamos el suelo
        if (player.IsGrounded())
        {
            // Si el jugador está presionando A o D al aterrizar, transita a MoveState
            if (Mathf.Abs(player.MovementInput.x) > 0.1f)
            {
                player.StateMachine.ChangeState(player.MoveState);
            }
            // Si el jugador no está tocando nada, transita a IdleState
            else
            {
                player.StateMachine.ChangeState(player.IdleState);
            }
        }
        // Verificamos si se presionó el botón Y si ya pasó el tiempo de recarga
        if (player.DashInput && Time.time >= player.LastDashTime + player.dashCooldown)
        {
            player.StateMachine.ChangeState(player.DashState);
            return;
        }
        // Si el jugador presionó el botón pero el cooldown no ha terminado, consumimos el input para que no se guarde
        else if (player.DashInput)
        {
            player.DashInput = false;
        }
    }

    public override void PhysicsUpdate()
    {
        // CONTROL AÉREO: Permite al jugador moverse de izquierda a derecha mientras cae.
        // Mantenemos la velocidad actual en Y para que la gravedad de Unity siga haciendo su trabajo.
        player.Rb.linearVelocity = new Vector2(player.MovementInput.x * player.moveSpeed, player.Rb.linearVelocity.y);
    }

    public override void Exit()
    {
        Debug.Log("Exiting Fall State");
    }
}
