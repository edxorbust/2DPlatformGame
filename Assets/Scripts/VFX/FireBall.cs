using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class FireBall : MonoBehaviour
{
    Transform attacker;
    [Header("Ajustes del Proyectil")]
    public float velocidad = 8f;
    public int dano = 15;
    [Tooltip("Tiempo en segundos antes de que desaparezca si no choca con nada")]
    public float tiempoDeVida = 5f; 
    public float fuerzaKnockback = 5f;
    public bool deflected;
    public Transform attackerTransform;

    [Header("Efectos Visuales (Opcional)")]
    public GameObject efectoExplosionPrefab;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Blindaje AAA: Nos aseguramos de que la gravedad no lo afecte
        rb.gravityScale = 0f;
        
        // El temporizador de autodestrucción inicia apenas el proyectil nace
        Destroy(gameObject, tiempoDeVida);
    }

    // Este es el método público que tu FlyingEnemyController llamará al disparar
    public void Disparar(Vector2 direccion, Transform attacker1)
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();

        // 1. Le damos el impulso en línea recta
        rb.linearVelocity = direccion.normalized * velocidad;

        // 2. Rotamos el sprite matemáticamente para que la bola mire hacia donde viaja
        float angulo = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angulo);

        attacker = attacker1;
    }

    public void Deflecting(Vector2 direccion, float velocidad, Transform attacker1)
    {
        deflected = true;
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = direccion.normalized * velocidad;

        // 2. Rotamos el sprite matemáticamente para que mire hacia donde viaja
        float angulo = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angulo);

        attackerTransform = attacker1;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (deflected)
        {
            if (collision.isTrigger) return;
            if (collision.CompareTag("Enemy"))
            {
                FlyingEnemyController flyer = collision.GetComponent<FlyingEnemyController>();
                
                if (flyer != null && flyer.StateMachine.CurrentState != flyer.HurtState)
                {
                    Transform origenDano = (attackerTransform != null) ? attackerTransform : transform;
                    float direccionEmpuje = (flyer.transform.position.x > origenDano.position.x) ? 1f : -1f;
                    flyer.GetAttack(dano, origenDano, true, direccionEmpuje);
                    
                    
                    
                }
                
                // Si le da al jugador, normalmente desaparece al instante
                deflected = false;
                Explotar();
            }
            return;
        }
        // 1. Filtros de seguridad: ignoramos a otros enemigos o a los gatillos (Triggers de cámara)
        if (collision.CompareTag("Enemy") || collision.isTrigger) return;

        // 2. Si golpeamos al jugador, aplicamos la misma lógica de daño de tu EnemyController
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            
            // Validamos que exista y que no esté ya recibiendo daño (invulnerable temporalmente)
            if (player != null && player.StateMachine.CurrentState != player.HurtState)
            {
                // Le pasamos el daño y el transform (quién lo atacó)
                attacker.GetComponent<EnemyController>().unBlockableAttacking = true;
                player.GetAttack(dano, attacker, true);
                attacker.GetComponent<EnemyController>().unBlockableAttacking = false;
                
                if(player.StateMachine.CurrentState != player.BlockState)
                {
                     // Aplicamos el Knockback basado en tu lógica original
                float direccionEmpuje = (player.transform.position.x > transform.position.x) ? 1f : -1f;
                player.Rb.linearVelocity = new Vector2(direccionEmpuje * fuerzaKnockback, player.Rb.linearVelocity.y);
                }
            }
        }

        // 3. Ya sea que golpeó al jugador, al suelo o a una pared, el proyectil se destruye
        Explotar();
    }

    private void Explotar()
    {
        // Si le asignaste partículas de explosión en el inspector, las crea justo antes de morir
        if (efectoExplosionPrefab != null)
        {
            Instantiate(efectoExplosionPrefab, transform.position, Quaternion.identity);
        }
        
        // Destruimos la bola de fuego
        Destroy(gameObject);
    }
}