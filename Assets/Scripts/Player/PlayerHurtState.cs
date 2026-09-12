using UnityEngine;

public class PlayerHurtState : BaseState // O el nombre que tenga tu clase base
{
    private PlayerController player;
    public float hitDirection;
    public SwordsmanController swordsman;

    public PlayerHurtState(PlayerController playerContext)
    {
        player = playerContext;
    }

    public override void Enter()
    {
        // 1. Reproducimos la animación
        player.Animator.Play("Hurt"); // Asegúrate de que el nombre coincida en Unity

        // 2. Frenamos al personaje para que no resbale (conservando su Y por si lo golpean en el aire)
        //player.Rb.linearVelocity = new Vector2(0f, player.Rb.linearVelocity.y);

        // APLICAMOS EL KNOCKBACK AL JUGADOR
            if (player.Rb != null)
            {
                // Si el enemigo está a la derecha, nos empuja a la izquierda (-1), y viceversa (1)
                
                // DOWNCASTING: Leemos la fuerza de retroceso si es un espadachín
                float force = swordsman != null ? swordsman.blockKnockbackForce : 2f;

                player.Rb.linearVelocity = new Vector2(hitDirection * force, player.Rb.linearVelocity.y);
            }

        // 3. Consumimos cualquier input que el jugador haya presionado por pánico
        player.LightAttackInput = false;
        player.HeavyAttackInput = false;
        player.JumpInput = false;
    }

    public override void Update()
    {
        // El Update está VACIÓ a propósito. 
        // Mientras esté aquí, no puede moverse, ni atacar, ni saltar. ¡Está sufriendo daño!
    }

    // Usamos el mismo método que creamos para el combo de ataques
    public override void AnimationFinishTrigger()
    {
        // Cuando la animación de Hurt termine, volvemos a Idle y le devolvemos el control
        player.StateMachine.ChangeState(player.IdleState);
    }
}