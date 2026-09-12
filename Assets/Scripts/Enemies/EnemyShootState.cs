using UnityEngine;

public class EnemyShootState : BaseState 
{
    private EnemyController enemy; 

    public EnemyShootState(EnemyController enemyContext) 
    {
        enemy = enemyContext; 
    }

    public override void Enter() 
    {
        enemy.cannotBeInterrupted = true; 
        
        // Detenemos al enemigo para que dispare firmemente desde el suelo
        enemy.Rb.linearVelocity = new Vector2(0f, enemy.Rb.linearVelocity.y); 
        
        // Obligamos al enemigo a mirar al jugador justo antes de tensar el arco
        enemy.FacePlayer();
        
        enemy.Animator.Play("LightAttack1"); 
    }

    public override void Update() { } 

    public void OnAnimationImpactFrame() 
    {
        // Se deja vacío porque el ArcherController ya se encarga de instanciar 
        // y disparar el prefab de la flecha en este frame exacto.
    }

    public void OnAnimationEndFrame() 
    {
        // El arquero usa secuencias, así que en lugar de evaluar un combo interno,
        // directamente pasamos al siguiente movimiento de su tarjeta.
        enemy.EjecutarSiguienteMovimiento(); 
    }

    public override void PhysicsUpdate() { } 

    public override void Exit() 
    {
        enemy.cannotBeInterrupted = false; 
    }
}