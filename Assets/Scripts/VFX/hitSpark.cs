using UnityEngine;

public class SelfDestroy : MonoBehaviour
{
    void Start()
    {
        // Destruye este objeto 0.3 segundos después de aparecer
        Destroy(gameObject, 0.3f); 
    }
}