using UnityEngine;

public class PlayerIdleState : BaseState
{
    // Referencia al script principal del jugador para acceder a sus componentes
    private PlayerController player;

    // Constructor: Cuando creamos este estado, le pasamos el jugador
    public PlayerIdleState(PlayerController playerContext)
    {
        player = playerContext;
    }

    public override void Enter()
    {

        // Detenemos por completo el movimiento horizontal del jugador.
        // Mantenemos la velocidad en Y intacta por si cae al suelo.
        player.Rb.linearVelocity = new Vector2(0f, player.Rb.linearVelocity.y);

        // Aquí en el futuro llamaremos a: player.Animator.Play("Idle");
        player.Animator.Play("Idle");
    }

    public override void Update()
    {
        // LECTURA DE TRANSICIONES:

        if (player.IsBlockButtonHeld)
        {
            player.StateMachine.ChangeState(player.BlockState);
            return;
        }
        // Si el jugador presiona una tecla de movimiento (A o D), cambiamos al estado de correr.
        if (Mathf.Abs(player.MovementInput.x) > 0.1f)
        {
            player.StateMachine.ChangeState(player.MoveState); // Lo descomentaremos cuando exista MoveState
        }

        // Si el jugador presiona salto, cambiamos al estado de salto.
        if (player.JumpInput && player.IsGrounded())
        {
            player.StateMachine.ChangeState(player.JumpState); // Lo descomentaremos cuando exista JumpState
        }

        if (!player.IsGrounded())
        {
            player.StateMachine.ChangeState(player.FallState);
            return; // Salimos para no ejecutar más código
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

        if (player.HeavyAttackInput && player.IsGrounded())
        {
            player.StateMachine.ChangeState(player.HeavyAttackState);
            return;
        }
        // Verificamos si se presionó el botón Y si ya pasó el tiempo de recarga
        if (player.DashInput && Time.time >= player.LastDashTime + player.dashCooldown)
        {
            player.StateMachine.ChangeState(player.DashState);
            return;
        }

        if (player.HealInput && player.Health.HealPotions > 0)
        {
            player.StateMachine.ChangeState(player.HealState);
        }
        // Si el jugador presionó el botón pero el cooldown no ha terminado, consumimos el input para que no se guarde
        else if (player.DashInput)
        {
            player.DashInput = false;
        }
    }

    public override void PhysicsUpdate()
    {
        float inerciaPlataformaX = 0f;
        if (player.PlataformaActual != null)
        {
            inerciaPlataformaX = player.PlataformaActual.VelocidadActual.x;
        }

        // 3. Aplicamos ambas velocidades sumadas, respetando la gravedad en Y
        player.Rb.linearVelocity = new Vector2(inerciaPlataformaX, player.Rb.linearVelocity.y);
    }

    public override void Exit()
    {
        // Se ejecuta justo antes de cambiar a moverse, saltar o atacar.
    }
}
