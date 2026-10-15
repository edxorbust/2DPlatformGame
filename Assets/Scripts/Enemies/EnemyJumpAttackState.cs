using UnityEngine;

public class EnemyJumpAttackState : BaseState
{
    private SwordsmanController enemy;
    private float jumpForce = 11f; // Ajusta este valor en tu inspector/controlador
    private bool animationEnded;
    private float gravedadOriginal;
    private int counter;
    private bool attackingStarted;

    public EnemyJumpAttackState(SwordsmanController enemyContext)
    {
        enemy = enemyContext;
    }

    public override void Enter()
    {
       // Reiniciamos las variables al entrar al estado
        attackingStarted = false;
        animationEnded = false;
        counter = 0;
        
        // Guardamos la gravedad original para restaurarla después
        gravedadOriginal = enemy.Rb.gravityScale;

        enemy.unParryableAttacking = true;
        enemy.cannotBeInterrupted = true;

        // Frena cualquier movimiento horizontal y aplica el salto (simplificado)
        enemy.Rb.linearVelocity = new Vector2(0, jumpForce);
        enemy.Animator.Play("Jump");
    }

    public override void Update()
    {
        if (animationEnded)
        {
            enemy.Rb.gravityScale = gravedadOriginal;
            enemy.StateMachine.ChangeState(enemy.FallState);
            return;
        }
        // Verificar cuando llega a su punto máximo de altura y asegurarnos de solo llamarlo una vez
        if (enemy.Rb.linearVelocity.y <= 0 && !attackingStarted)
        {
            attackingStarted = true;
            // CONGELAMOS AL ENEMIGO EN EL AIRE (opcional, pero da un efecto dramático muy bueno)
            enemy.Rb.linearVelocity = Vector2.zero;
            enemy.Rb.gravityScale = 0f;

            
            
            // Damos play a la animación de windup/dash attack
            if (enemy.undodgableAttackSign != null)
            {
                GameObject.Instantiate(enemy.undodgableAttackSign, enemy.warningsPosition.position , Quaternion.identity);
            }
            enemy.Animator.Play("AirAttack");
        }
    }

    public override void WindupFinishedTrigger()
    {
        // Le damos permiso a PhysicsUpdate para que empiece a moverlo
        //enemy.FacePlayer();
        enemy.ReproducirSonido(enemy.sonidoAtaque);
    }

    public void OnAnimationImpactFrame()
{
    bool connectedHit = false;
    bool facingRight = enemy.transform.localScale.x >= 1;

    Collider2D[] hits = Physics2D.OverlapCircleAll(
        enemy.transform.position,
        enemy.detectionRange,
        enemy.playerLayer
    );

    foreach (Collider2D hit in hits)
    {
        Vector2 targetPosition = hit.bounds.center;
        // Ignoramos cualquier collider que esté detrás del enemigo
         if (!IsInsideAttackHalfCircle(targetPosition, facingRight))
            {
                Debug.Log("ningun jugador detectado");
                continue;
            }    

        PlayerController playerController = hit.GetComponent<PlayerController>();

        if (playerController != null)
        {
            enemy.ReproducirSonido(enemy.fleshCut);

            if (enemy.hitSparkPrefab != null)
            {
                GameObject.Instantiate(
                    enemy.hitSparkPrefab,
                    playerController.transform.position,
                    Quaternion.identity
                );
            }

            

            playerController.GetAttack(
                enemy.attackDamage,
                enemy.transform
            );

            connectedHit = true;
        }
        else
        {
            BaseEntity entity = hit.GetComponent<BaseEntity>();

            if (entity != null)
            {
                entity.Damage(enemy.attackDamage);
            }
        }

        if (connectedHit)
        {
            enemy.TriggerHitStop(enemy.lightHitStopDuration);
        }
    }
}

private bool IsInsideAttackHalfCircle(
    Vector2 targetPosition,
    bool facingRight)
{
    float centerX = enemy.transform.position.x;

    if (facingRight)
    {
        return targetPosition.x >= centerX;
    }
    else
    {
        return targetPosition.x <= centerX;
    }
}

    public override void AnimationFinishTrigger()
    {
        if (!attackingStarted)
        {
            return;
        }
        if (counter < 5)
        {
            
            
            AnimatorStateInfo stateInfo = enemy.Animator.GetCurrentAnimatorStateInfo(0);

            if (stateInfo.IsName("AirAttack"))
            {
                counter++;
                enemy.FacePlayer();
                enemy.Animator.Play("Jump");
            }
            else if (stateInfo.IsName("Jump"))
            {
                enemy.Animator.Play("AirAttack");
            }
        }
        else
        {
            animationEnded = true;
            attackingStarted = false;
        }
    }

    public override void Exit()
    {
        // MUY IMPORTANTE: Devolverle la gravedad al enemigo al salir del estado
        enemy.Rb.gravityScale = gravedadOriginal;
        enemy.unParryableAttacking = false;
        enemy.cannotBeInterrupted = false;
    }

}
