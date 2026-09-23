using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Animator))] // Añadimos este requerimiento
public class ArrowProjectile : MonoBehaviour
{
    [Header("Estadísticas de Combate")]
    public int damage = 10;
    public float knockbackForce = 5f;
    public float lifeTime = 4f;
    public bool deflected;

    [Header("Detección de Suelo y Animación")]
    public LayerMask capaSuelo;
    [Tooltip("Nombre exacto del estado de animación en el Animator")]
    public string animacionClavado = "Hit"; 

    private Transform attackerTransform;
    private Rigidbody2D rb;
    private Animator animator;
    private Collider2D col;
    
    // Bandera para saber si ya chocó con el suelo
    private bool isStuck = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        col = GetComponent<Collider2D>();
    }

    private void Start()
    {
        // Temporizador base por si se va volando fuera del mapa
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        if (deflected)
        {
            return;
        }
        // Solo calculamos la rotación si la flecha sigue volando (no está clavada)
        if (!isStuck && rb != null && rb.linearVelocity.sqrMagnitude > 0.1f)
        {
            float angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    public void ConfigurarFlecha(Vector2 direccion, float velocidad, Transform atacante)
    {
        attackerTransform = atacante;
        
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = direccion * velocidad;
    }

    public void Deflecting(Vector2 direccion, float velocidad, Transform attacker1)
    {
        deflected = true;
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
            if (isStuck || collision.isTrigger) return;
            if (collision.CompareTag("Enemy"))
            {
                ArcherController archer = collision.GetComponent<ArcherController>();
                
                if (archer != null && archer.StateMachine.CurrentState != archer.HurtState)
                {
                    Transform origenDano = (attackerTransform != null) ? attackerTransform : transform;
                    float direccionEmpuje = (archer.transform.position.x > origenDano.position.x) ? 1f : -1f;
                    archer.GetAttack(damage, origenDano, true, direccionEmpuje);
                    
                    
                    
                }
                
                // Si le da al jugador, normalmente desaparece al instante
                deflected = false;
                Destroy(gameObject); 
            }
            else if (((1 << collision.gameObject.layer) & capaSuelo) != 0)
            {
                deflected = false;
                ClavarFlecha();
            }
            return;
        }
        // Si ya está clavada, o choca con triggers/enemigos, ignoramos
        if (isStuck || collision.isTrigger || collision.CompareTag("Enemy")) return;

        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            
            if (player != null && player.StateMachine.CurrentState != player.HurtState)
            {
                Transform origenDano = (attackerTransform != null) ? attackerTransform : transform;

                origenDano.GetComponent<EnemyController>().unBlockableAttacking = true;
                player.GetAttack(damage, origenDano, true);
                origenDano.GetComponent<EnemyController>().unBlockableAttacking = false;
                
                float direccionEmpuje = (player.transform.position.x > origenDano.position.x) ? 1f : -1f;
                player.Rb.linearVelocity = new Vector2(direccionEmpuje * knockbackForce, player.Rb.linearVelocity.y);
            }
            
            // Si le da al jugador, normalmente desaparece al instante
            Destroy(gameObject); 
        }
        else if (((1 << collision.gameObject.layer) & capaSuelo) != 0)
        {
            ClavarFlecha();
        }
    }

    private void ClavarFlecha()
    {
        isStuck = true;

        // 1. Frenamos la flecha y desactivamos la gravedad para que se quede en su lugar
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        // 2. Apagamos el colisionador
        if (col != null) col.enabled = false;

        // 3. Reproducimos tu animación (tambaleo)
        if (animator != null)
        {
            animator.Play(animacionClavado);
        }

        // 4. Sobrescribimos la destrucción para darle tiempo de tambalearse y quedarse ahí un rato
        // Cambia este '2f' por el tiempo que quieras que el cuerpo de la flecha dure en pantalla
        Destroy(gameObject, 2f); 
    }
}