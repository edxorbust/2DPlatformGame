using UnityEngine;
using Unity.Cinemachine; // O "using Cinemachine;" si usas una versión anterior

public class ControlZoneCamera : MonoBehaviour
{
    [Header("La cámara de esta habitación")]
    public CinemachineCamera camaraDeZona; // En Unity 6+ podría llamarse CinemachineCamera

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Cuando el jugador entra, esta cámara toma el control absoluto
            camaraDeZona.Priority = 20;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Cuando el jugador sale, la cámara vuelve a ser secundaria
            camaraDeZona.Priority = 10;
        }
    }
}