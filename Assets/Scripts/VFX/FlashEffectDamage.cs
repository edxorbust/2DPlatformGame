using System.Collections;
using UnityEngine;

public class FlashEffectDamage : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Material materialOriginal;
    
    [Header("Configuración del Flasheo")]
    public Material materialBlanco; // El material que crearemos en Unity
    public float duracionFlasheo = 0.1f; // 0.1 a 0.15 segundos es el estándar AAA

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        // Guardamos el material normal del enemigo al inicio
        materialOriginal = spriteRenderer.material; 
    }

    // Este es el método que llamarás cuando el enemigo reciba daño
    public void EjecutarFlasheo()
    {
        StartCoroutine(RutinaFlasheo());
    }

    private IEnumerator RutinaFlasheo()
    {
        // 1. Pintamos el sprite de blanco puro
        spriteRenderer.material = materialBlanco;

        // 2. Esperamos una fracción de segundo
        yield return new WaitForSeconds(duracionFlasheo);

        // 3. Restauramos los colores originales
        spriteRenderer.material = materialOriginal;
    }
}
