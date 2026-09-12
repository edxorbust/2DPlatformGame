using UnityEngine;

public class SpeedChangePlatform : MonoBehaviour
{
    private bool speedChanged = false;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !speedChanged)
        {
            PlayerController jugador = collision.GetComponent<PlayerController>();
            PlataformaModular plataforma = jugador.PlataformaActual;
            if (plataforma != null)
            {
                plataforma.velocidadGuardada = plataforma.velocidad;
                plataforma.velocidad = plataforma.velocidadRapida;
                speedChanged = true;
            }
        }
    }
}
