using UnityEngine;

public class PlayerHurtState : BaseState // O el nombre que tenga tu clase base
{
    private PlayerController player;
    public float hitDirection;
    public SwordsmanController swordsman;
    private float timeBeingPushed = 0.5f;
    private float actualTimePushed;

    public PlayerHurtState(PlayerController playerContext)
    {
        player = playerContext;
    }

    public override void Enter()
    {
        

        // 2. Frenamos al personaje para que no resbale (conservando su Y por si lo golpean en el aire)
        //player.Rb.linearVelocity = new Vector2(0f, player.Rb.linearVelocity.y);
        hitDirection = player.hurtAndPushDirection;
        if (player.beingPushed)
        {
            Debug.Log("entrando a push");
            player.Animator.Play("Pushed");
            actualTimePushed = timeBeingPushed;
            if (player.Rb != null)
            {
                // Si el enemigo está a la derecha, nos empuja a la izquierda (-1), y viceversa (1)
                
                // DOWNCASTING: Leemos la fuerza de retroceso si es un espadachín

                player.Rb.linearVelocity = new Vector2(hitDirection * 8f, player.Rb.linearVelocity.y);
            }
        }
        else if (player.pushedUp)
        {
            player.Animator.Play("Pushed");
            actualTimePushed = timeBeingPushed;
            if (player.Rb != null)
            {
                // Si el enemigo está a la derecha, nos empuja a la izquierda (-1), y viceversa (1)
                
                // DOWNCASTING: Leemos la fuerza de retroceso si es un espadachín

                player.Rb.linearVelocity = new Vector2(hitDirection * 6f, 8f);
            }
        }
        else
        {
            player.Animator.Play("Hurt");
            // if (player.Rb != null)
            // {
            //     // Si el enemigo está a la derecha, nos empuja a la izquierda (-1), y viceversa (1)
                
            //     // DOWNCASTING: Leemos la fuerza de retroceso si es un espadachín

            //     player.Rb.linearVelocity = new Vector2(hitDirection * 2f, player.Rb.linearVelocity.y);
            // }
        }
        // 3. Consumimos cualquier input que el jugador haya presionado por pánico
        player.LightAttackInput = false;
        player.HeavyAttackInput = false;
        player.JumpInput = false;
    }

    public override void Update()
    {
        if (player.beingPushed || player.pushedUp)
        {
            actualTimePushed -= Time.deltaTime;
            if (actualTimePushed <= 0)
            {
                player.StateMachine.ChangeState(player.IdleState);
            }
        }
    }

    // Usamos el mismo método que creamos para el combo de ataques
    public override void AnimationFinishTrigger()
    {
        // Cuando la animación de Hurt termine, volvemos a Idle y le devolvemos el control
        if (player.beingPushed || player.pushedUp)
        {
            
        }
        else
        {
            player.StateMachine.ChangeState(player.IdleState);
        }
    }

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
    }
}