using UnityEngine;

public class PlayerBlockState : BaseState
{
    private PlayerController player;
    private float parryTimer;
    private bool parrySuccessful;

    public PlayerBlockState(PlayerController playerContext)
    {
        player = playerContext;
    }

    public override void Enter()
    {
        parrySuccessful = false;
        parryTimer = player.parryDuration;
        
        player.Rb.linearVelocity = new Vector2(0f, player.Rb.linearVelocity.y);

        // Iniciamos en Fase 1: Parry Perfecto
        player.Health.IsParrying = true;
        player.Health.IsBlocking = false;

        player.Health.DoingParry += OnParrySuccess;
        player.Animator.Play("Block");
    }

    public override void Update()
    {
        if (parrySuccessful) return;

        // 1. CONDICIÓN DE SALIDA: Si el jugador suelta el botón, cancelamos la defensa
        if (!player.IsBlockButtonHeld)
        {
            player.StateMachine.ChangeState(player.IdleState);
            return;
        }

        // 2. MANEJO DEL TIEMPO (Fase 1 a Fase 2)
        if (player.Health.IsParrying)
        {
            parryTimer -= Time.deltaTime;

            // Se acabó la ventana perfecta, pasamos a Bloqueo Normal
            if (parryTimer <= 0f)
            {
                Debug.Log("Ventana de Parry cerrada. Entrando a Bloqueo Normal.");
                player.Health.IsParrying = false;
                player.Health.IsBlocking = true; 
            }
        }
    }

    public override void PhysicsUpdate()
    {
        
        if (player.PlataformaActual != null)
        {
            float inerciaPlataformaX = player.PlataformaActual.VelocidadActual.x;
            // 3. Aplicamos ambas velocidades sumadas, respetando la gravedad en Y
            player.Rb.linearVelocity = new Vector2(inerciaPlataformaX, player.Rb.linearVelocity.y);
        }
    }

    public override void Exit()
    {
        // Apagamos todas las defensas al salir
        player.Health.IsParrying = false;
        player.Health.IsBlocking = false;
        player.Health.DoingParry -= OnParrySuccess;
    }

    private void OnParrySuccess()
    {
        parrySuccessful = true;
        player.StateMachine.ChangeState(player.IdleState);
    }
}