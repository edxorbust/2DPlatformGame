using UnityEngine;

public class PlayerLightAttackState : BaseState
{
    private PlayerController player;
    private bool continueWithLight;

    public PlayerLightAttackState(PlayerController playerContext)
    {
        player = playerContext;
    }

    public override void Enter()
    {
        if(player.comboStep >= 3)
        {
            player.comboStep = 0;
        }
        if (player.IsGrounded())
        {
            // Frenamos al personaje horizontalmente para darle peso al golpe, 
            // pero mantenemos la gravedad en Y por si cae mientras ataca.
            player.Rb.linearVelocity = new Vector2(0f, player.Rb.linearVelocity.y);
        }
        player.comboStep++;
        Debug.Log($"Entering Light Attack - Combo Step: {player.comboStep}");
        player.ReproducirSonido(player.sonidoEspadazo);
        player.Animator.Play($"LightAttack{player.comboStep}");
        
        // Consumimos el input para no atacar dos veces por accidente
        player.LightAttackInput = false;
        player.HeavyAttackInput = false;
        continueWithLight = false;

    }

    public override void AnimationImpactTrigger()
    {
        // 1. LANZAR EL RADAR DE DAÑO
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(player.attackPoint.position, player.attackRange, player.enemyLayers);

        // Interruptor para saber si conectamos el golpe
        bool connectedHit = false;

        // 2. APLICAR DAÑO A CADA ENEMIGO ALCANZADO
        foreach (Collider2D enemy in hitEnemies)
        {
            // Buscamos si es un enemigo con cerebro (Base)
            EnemyController enemyController = enemy.GetComponent<EnemyController>();
            
            if (enemyController != null)
            {
                
                enemyController.GetAttack(player.GetLightAttackDamage(), player.transform);
                connectedHit = true;
            }
            else
            {
                // Si no tiene cerebro (ej. una caja rompible), solo le hacemos daño normal
                BaseEntity entity = enemy.GetComponent<BaseEntity>();
                if (entity != null)
                {
                    entity.Damage(player.GetLightAttackDamage());
                    connectedHit = true;
                }
            }
        }
        
        if (connectedHit)
        {
            player.TriggerHitStop(player.lightHitStopDuration);
        }
    }

    public override void Update()
    {

        // 3. INPUT BUFFERING: Escuchamos durante el ataque
        if (player.LightAttackInput)
        {
            continueWithLight = true;
            player.LightAttackInput = false; // Consumimos el input
        }
    }

    public override void AnimationFinishTrigger()
    {
        // Si presionó Ligero durante la animación, y no nos hemos pasado de golpes
        if (continueWithLight && player.comboStep < player.maxComboSteps)
        {
            player.StateMachine.ChangeState(player.LightAttackState);
        }
        // Si no presionó nada, el combo termina
        else
        {
            player.comboStep = 0; 
            player.StateMachine.ChangeState(player.IdleState);
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
        Debug.Log("Exiting Light Attack State");
    }
}
