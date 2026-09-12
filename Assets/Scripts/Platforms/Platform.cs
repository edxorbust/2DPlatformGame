using System.Collections;
using UnityEngine;

public class PlataformaModular : MonoBehaviour
{
    public enum TipoPlataforma { Infinita, MoverAlPisar, CaidaTemporal, UnSoloViaje }

    [Header("Configuración Principal")]
    public TipoPlataforma tipoDePlataforma;
    public float velocidad = 3f;
    public float velocidadRapida = 5f;
    
    public Transform puntoDestino; 
    
    [Header("Ajustes de Caída")]
    public float tiempoParaCaer = 1f;
    public float tiempoParaReaparecer = 3f;

    // ESTA ES LA VARIABLE CLAVE QUE LEERÁ EL JUGADOR
    public Vector2 VelocidadActual { get; private set; } 

    private Rigidbody2D rb;
    private Vector2 posicionA; 
    private Vector2 posicionB; 
    private Vector2 destinoActual;

    private bool jugadorEncima = false;
    private bool cayendo = false;
    private Vector2 posicionInicial;

    private Collider2D colisionador;
    private SpriteRenderer renderizador;
    private Animator anim; 
    private bool regresoActivado = false;
    public float velocidadGuardada;

    private void Start()
    {
        colisionador = GetComponent<Collider2D>();
        renderizador = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>(); 
        rb = GetComponent<Rigidbody2D>(); 

        posicionA = transform.position;
        posicionInicial = posicionA;

        if (puntoDestino != null) 
        {
            posicionB = puntoDestino.position;
            destinoActual = posicionB;
        }
    }

    private void FixedUpdate()
    {
        if (cayendo) 
        {
            VelocidadActual = Vector2.zero;
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (regresoActivado)
        {
            MoverPlataforma(false);
            if (Vector2.Distance(rb.position, destinoActual) < 0.1f)
            {
                regresoActivado = false;
                destinoActual = puntoDestino.position;
            }
            return;
        }

        switch (tipoDePlataforma)
        {
            case TipoPlataforma.Infinita:
                MoverPlataforma(true);
                break;
            case TipoPlataforma.MoverAlPisar:
                if (jugadorEncima) MoverPlataforma(true);
                else DetenerPlataforma();
                break;
            case TipoPlataforma.UnSoloViaje:
                if (jugadorEncima) MoverPlataforma(false); 
                else if (Vector2.Distance(rb.position, destinoActual) > 0.05f) DetenerPlataforma();
                break;
            case TipoPlataforma.CaidaTemporal:
                VelocidadActual = Vector2.zero;
                break;
        }
    }

    private void MoverPlataforma(bool idaYVuelta)
    {
        if (puntoDestino == null) return;

        // Calculamos la dirección y aplicamos la velocidad física pura
        Vector2 direccion = (destinoActual - rb.position).normalized;
        VelocidadActual = direccion * velocidad;
        rb.linearVelocity = VelocidadActual;

        if (Vector2.Distance(rb.position, destinoActual) < 0.1f)
        {
            if (idaYVuelta) 
            {
                destinoActual = (destinoActual == posicionA) ? posicionB : posicionA;
            }
            else
            {
                DetenerPlataforma();
            }
        }
    }

    public void RegresarAOrigen()
    {
        destinoActual = posicionInicial;
        velocidad = velocidadGuardada;
        regresoActivado = true;

    }

    private void DetenerPlataforma()
    {
        VelocidadActual = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && collision.GetContact(0).normal.y <= -0.5f)
        {
            jugadorEncima = true;
            if (tipoDePlataforma == TipoPlataforma.CaidaTemporal && !cayendo)
            {
                StartCoroutine(RutinaCaida());
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player")) jugadorEncima = false;
    }

    private IEnumerator RutinaCaida()
    {
        cayendo = true;
        if (anim != null) anim.SetTrigger("Temblar");
        yield return new WaitForSeconds(tiempoParaCaer);

        jugadorEncima = false;
        colisionador.enabled = false;
        renderizador.enabled = false;

        if (tiempoParaReaparecer > 0f)
        {
            yield return new WaitForSeconds(tiempoParaReaparecer);
            transform.position = posicionInicial;
            rb.position = posicionInicial; 
            colisionador.enabled = true;
            renderizador.enabled = true;
            cayendo = false;
            if (anim != null) anim.SetTrigger("Reaparecer");
        }
    }
}