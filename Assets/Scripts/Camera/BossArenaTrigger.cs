using UnityEngine;
using Unity.Cinemachine; // Usa "using Cinemachine;" si tu versión es anterior

public class BossArenaTrigger : MonoBehaviour
{
    [Header("Cámara de la Arena")]
    public CinemachineCamera camaraDelJefe;

    [Header("Puertas / Paredes Invisibles")]
    public GameObject[] paredesInvisibles;

    private bool arenaActivada = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Si el jugador entra y la arena no se ha activado aún
        if (collision.CompareTag("Player") && !arenaActivada)
        {
            arenaActivada = true;
            IniciarPeleaDeJefe();
        }
    }

    private void IniciarPeleaDeJefe()
    {
        // 1. Forzamos a la cámara del jefe a tomar el control absoluto
        // Le damos una prioridad altísima (ej: 30) para ganarle a las cámaras de exploración
        if (camaraDelJefe != null)
        {
            camaraDelJefe.Priority = 30;
        }

        // 2. Encendemos todas las paredes invisibles de golpe
        foreach (GameObject pared in paredesInvisibles)
        {
            pared.SetActive(true);
        }

        // 3. (Opcional) Aquí es donde llamarías al script de tu jefe para que despierte
        // jefeScript.Despertar();
        Debug.Log("¡El Jefe ha despertado y las puertas están cerradas!");
    }

    public void FinalizarPeleaDeJefe()
    {
        // 1. Apagamos todas las paredes invisibles
        foreach (GameObject pared in paredesInvisibles)
        {
            if (pared != null)
            {
                pared.SetActive(false);
            }
        }

        // 2. Le quitamos la prioridad a la cámara del jefe para que la cámara normal regrese suavemente
        if (camaraDelJefe != null)
        {
            camaraDelJefe.Priority = 10;
        }

        Debug.Log("¡El Jefe ha sido derrotado y las puertas se abrieron!");
    }
}