using UnityEngine;
using Unity.Cinemachine; // Si usas una versión antigua de Cinemachine, usa "using Cinemachine;"

public class TransitionCamera : MonoBehaviour
{
    [Header("Cámaras a intercambiar")]
    public GameObject camaraEncender;
    public GameObject camaraApagar;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Apagamos la cámara anterior y encendemos la nueva
            camaraEncender.SetActive(true);
            camaraApagar.SetActive(false);
        }
    }
}