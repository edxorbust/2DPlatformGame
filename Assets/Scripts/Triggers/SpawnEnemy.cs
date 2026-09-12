using Unity.VisualScripting;
using UnityEngine;

public class SpawnEnemy : MonoBehaviour
{
    [Header("Enemigos que aparecen")]
    public GameObject enemigo;
    [Tooltip("El punto donde aparecerán los prefabs (opcional)")]
    public Transform puntoDeAparicion;

    public bool seActivo = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !seActivo)
        {
            Debug.Log("apareciendo enemigo");
            seActivo = true;
            AparecerEnemigos();
        }
    }

    private void AparecerEnemigos()
    {
        if(enemigo != null)
        {
            Vector2 posicion = (puntoDeAparicion != null) ? (Vector2)puntoDeAparicion.position : (Vector2)transform.position;
                
            Instantiate(enemigo, posicion, Quaternion.identity);
        }
    }
}
