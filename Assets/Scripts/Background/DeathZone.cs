using UnityEngine;

public class DeathZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Verificamos si lo que cayó fue el jugador
        if (collision.CompareTag("Player"))
        {
            // 1. Buscamos el componente de salud de tu jugador
            BaseEntity health = collision.GetComponent<BaseEntity>();
            
            if (health != null)
            {
                // Le hacemos daño masivo para asegurarnos de que muera y 
                // se dispare tu evento Health.Dying
                health.EnviromentDamage(9999);
            }
        }
        
        // Opcional: Si los enemigos caen por accidente, puedes destruirlos
        else if (collision.CompareTag("Enemy"))
        {
            Destroy(collision.gameObject);
        }
    }
}