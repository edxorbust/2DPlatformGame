using UnityEngine;

public class EnemyDeathState : BaseState
{
    private EnemyController enemy;

    public EnemyDeathState(EnemyController enemyContext)
    {
        enemy = enemyContext;
    }

    public override void Enter()
    {
        Debug.Log("entering death state");
        enemy.Animator.Play("Death");
        enemy.Rb.linearVelocity = Vector2.zero;
        enemy.Rb.simulated = false; // Apagamos físicas
    }

    public override void AnimationFinishTrigger()
    {
        Debug.Log("Animación de muerte terminada. Enemigo abatido.");
        // Aquí llamas al menú de Game Over, reinicias la escena, etc.
        Object.Destroy(enemy.gameObject);
    }

    public override void PhysicsUpdate()
    {
        float inerciaPlataformaX = 0f;
        if (enemy.PlataformaActual != null)
        {
            inerciaPlataformaX = enemy.PlataformaActual.VelocidadActual.x;
        }

        // 3. Aplicamos ambas velocidades sumadas, respetando la gravedad en Y
        enemy.Rb.linearVelocity = new Vector2(inerciaPlataformaX, enemy.Rb.linearVelocity.y);
    }
}
