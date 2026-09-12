using UnityEngine;

public class PlayerStunnedState : BaseState
{
    private PlayerController player;
    private float stunTimer;

    public PlayerStunnedState(PlayerController playerContext)
    {
        player = playerContext;
    }

    public override void Enter()
    {
        Debug.Log("Jugador: ¡ME ROMPIERON LA POSTURA! Aturdido...");
        stunTimer = player.stunDuration;
        player.isStunned = true;

        // Frenamos al jugador por completo
        player.Rb.linearVelocity = new Vector2(0f, player.Rb.linearVelocity.y);


        player.Animator.Play("Stunned");
    }

    public override void Update()
    {
        stunTimer -= Time.deltaTime;

        if (stunTimer <= 0f)
        {
            // Termina el castigo, volvemos a Idle
            player.StateMachine.ChangeState(player.IdleState);
        }
    }

    public override void PhysicsUpdate() { }

    public override void Exit()
    {
        Debug.Log("Jugador: Recuperé el aliento.");

        player.isStunned = false;
        
        // ¡VITAL! Le devolvemos su postura al máximo para que pueda seguir bloqueando
        player.Health.RestartPosture();
    }
}