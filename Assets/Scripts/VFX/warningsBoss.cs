using UnityEngine;

public class SelfDestroyWarning : MonoBehaviour
{
    void Start()
    {
        // Destruye este objeto 0.3 segundos después de aparecer
        Destroy(gameObject, 0.6f); 
    }
}