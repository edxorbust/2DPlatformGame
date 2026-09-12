using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
public class PlayerDeathState : BaseState 
{
    private PlayerController player;

    public PlayerDeathState(PlayerController playerContext)
    {
        player = playerContext;
    }

    public override void Enter()
    {
        player.Animator.Play("Death");
    }

    public override void AnimationFinishTrigger()
    {
        Debug.Log("Animación de muerte terminada. Game Over.");
        Object.Destroy(player.gameObject);
        Time.timeScale = 0f;
        // Aquí llamas al menú de Game Over, reinicias la escena, etc.
    }
}