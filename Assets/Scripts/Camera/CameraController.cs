using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    // Esto nos permite llamar a la cámara desde CUALQUIER otro script fácilmente
    public static CameraController Instancia; 

    private Vector3 posicionOriginal;

    private void Awake()
    {
        Instancia = this;
    }

    public void AgitarCamara(float duracion, float magnitud)
    {
        // Guardamos la posición original justo antes de empezar a temblar
        posicionOriginal = transform.localPosition;
        StartCoroutine(RutinaAgitar(duracion, magnitud));
    }

    private IEnumerator RutinaAgitar(float duracion, float magnitud)
    {
        float tiempoTranscurrido = 0f;

        while (tiempoTranscurrido < duracion)
        {
            // Genera coordenadas aleatorias para el temblor
            float x = Random.Range(-1f, 1f) * magnitud;
            float y = Random.Range(-1f, 1f) * magnitud;

            // Movemos la cámara
            transform.localPosition = new Vector3(posicionOriginal.x + x, posicionOriginal.y + y, posicionOriginal.z);

            // ¡EL TRUCO VITAL! Usamos unscaledDeltaTime para que la cámara tiemble 
            // INCLUSO si el Hit-Stop tiene el juego congelado.
            tiempoTranscurrido += Time.unscaledDeltaTime; 
            
            yield return null; // Espera al siguiente frame
        }

        // Devolvemos la cámara a su lugar exacto al terminar
        transform.localPosition = posicionOriginal;
    }
}