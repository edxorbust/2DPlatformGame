using UnityEngine;

public class EnemyTouchDamage : MonoBehaviour
{
    private EnemyController enemy;

    private void Awake()
    {
        enemy = GetComponentInParent<EnemyController>();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null && player.StateMachine.CurrentState != player.HurtState && enemy.touchDamageActivated)
            {
                player.GetAttack(enemy.contactDamage, transform);
                float direccionEmpuje = (player.transform.position.x > transform.position.x) ? 1f : -1f;
                player.Rb.linearVelocity = new Vector2(direccionEmpuje * enemy.knockbackForceHurt, player.Rb.linearVelocity.y);
            }
        }
    }
}
