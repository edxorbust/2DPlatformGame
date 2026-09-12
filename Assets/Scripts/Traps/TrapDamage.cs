using UnityEngine;

public class TrapDamage : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController jugador = collision.GetComponent<PlayerController>();
            if (jugador != null)
            {
                jugador.RecibirDanoPorTrampa();
            }
        }
    }
}
