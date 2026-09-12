using UnityEngine;

public class EnemyHeavyShootState : BaseState 
{
    private EnemyController enemy; 

    public EnemyHeavyShootState(EnemyController enemyContext) 
    {
        enemy = enemyContext; 
    }

    public override void Enter() 
    {
        enemy.cannotBeInterrupted = true; 
        
        enemy.Rb.linearVelocity = new Vector2(0f, enemy.Rb.linearVelocity.y); 
        enemy.FacePlayer();
        
        enemy.Animator.Play("HeavyAttack"); 
    }

    public override void Update()
    {
    } 

    public void OnAnimationImpactFrame() 
    {
        // La matemática para calcular el abanico de las 3 flechas 
        // ya se ejecuta automáticamente en el ArcherController.
    }

    public void OnAnimationEndFrame() 
    {
        enemy.EjecutarSiguienteMovimiento(); 
    }

    public override void PhysicsUpdate() { } 

    public override void Exit() 
    {
        enemy.cannotBeInterrupted = false; 
    }
}