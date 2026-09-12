using UnityEngine;

public class LeverForPlatform : MonoBehaviour
{
    [Header("Plataforma que regresara")]
    public PlataformaModular plataforma;

    private Animator anim;
    private bool jugadorEnRango = false;
    private PlayerController jugador;

    private void Awake()
    {
        // 1. Buscamos y guardamos el Animator al iniciar
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        // 2. Revisamos constantemente si el jugador está en rango y apretó el botón
        if (jugadorEnRango && jugador != null)
        {
            if (jugador.InteractInput)
            {
                Debug.Log("activando lever");
                anim.Play("Activate");
                plataforma.RegresarAOrigen();

                // Consumimos el input para que no dispare la función 60 veces por segundo
                jugador.InteractInput = false; 
                
                // Opcional: si la palanca es de un solo uso, descomenta esta línea
                // this.enabled = false; 
            }
        }
    }

    public void TriggerFinishAnimation()
    {
        anim.Play("Idle");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // El jugador entra a la zona de la palanca
        if (collision.CompareTag("Player"))
        {
            jugador = collision.GetComponent<PlayerController>();
            jugadorEnRango = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // El jugador sale de la zona, ya no puede interactuar
        if (collision.CompareTag("Player"))
        {
            jugadorEnRango = false;
            jugador = null;
        }
    }
}
