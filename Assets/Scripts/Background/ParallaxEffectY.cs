using UnityEngine;

public class ParallaxEffectY : MonoBehaviour
{
    private float posicionInicialY;
    public Transform camaraPrincipal;
    
    [Header("Configuración")]
    [Tooltip("0 = Se mueve igual que el suelo. 1 = Se queda pegado a la cámara (cielo infinito)")]
    public float multiplicadorParallax;

    void Start()
    {
        // Guardamos dónde empezó la imagen al iniciar el juego
        posicionInicialY = transform.position.y;
        
        // Si olvidaste asignar la cámara, Unity la busca automáticamente
        if (camaraPrincipal == null) 
        {
            camaraPrincipal = Camera.main.transform;
        }
    }

    // Usamos LateUpdate en lugar de Update para evitar tirones (jitter)
    // porque la cámara se mueve primero, y el fondo reacciona DESPUÉS.
    void LateUpdate()
    {
        // Calculamos cuánto se ha movido la cámara desde el inicio
        float distancia = (camaraPrincipal.position.y * multiplicadorParallax);

        // Movemos el fondo en base a esa distancia
        transform.position = new Vector3(transform.position.x, posicionInicialY + distancia, transform.position.z);
    }
}
