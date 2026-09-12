using UnityEngine;

public class PlayerMoveState : BaseState
{
    private PlayerController player;

    public PlayerMoveState(PlayerController playerContext)
    {
        player = playerContext;
    }

    public override void Enter()
    {
        Debug.Log("Entering Move State");
        // En el futuro: player.Animator.Play("Run");
        player.Animator.Play("Run");
    }

    public override void Update()
    {
        if (player.IsBlockButtonHeld)
        {
            player.StateMachine.ChangeState(player.BlockState);
            return;
        }
        // TRANSICIÓN AL IDLE: Si el valor del input es casi cero (el jugador soltó A y D)
        if (Mathf.Abs(player.MovementInput.x) <= 0.1f)
        {
            player.StateMachine.ChangeState(player.IdleState);
        }

        // TRANSICIÓN AL SALTO: Si la bandera de salto está levantada
        if (player.JumpInput && player.IsGrounded())
        {
            player.StateMachine.ChangeState(player.JumpState); // Lo haremos pronto
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
        // Si el jugador presionó el botón pero el cooldown no ha terminado, consumimos el input para que no se guarde
        else if (player.DashInput)
        {
            player.DashInput = false;
        }
    }

    public override void PhysicsUpdate()
    {
        // 1. Calculamos la velocidad propia del jugador dictada por el input
        float velocidadJugadorX = player.MovementInput.x * player.moveSpeed; //
        float inerciaPlataformaX = 0f;

        // 2. Si el jugador está sobre una plataforma, extraemos la inercia de esta
        if (player.PlataformaActual != null)
        {
            inerciaPlataformaX = player.PlataformaActual.VelocidadActual.x;
        }

        // 3. Aplicamos ambas velocidades sumadas, respetando la gravedad en Y
        player.Rb.linearVelocity = new Vector2(velocidadJugadorX + inerciaPlataformaX, player.Rb.linearVelocity.y); //
    }

    public override void Exit()
    {
        Debug.Log("Exiting Move State");
    }
}