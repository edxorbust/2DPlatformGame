using UnityEngine;
using UnityEngine.UI; // ¡Crucial! Nos permite usar la palabra 'Image'
using TMPro;

public class PlayerUIManager : MonoBehaviour
{
    [Header("Referencias del Jugador")]
    public PlayerController player;

    [Header("UI Elementos")]
    // Esta variable guardará la referencia a tu texto en la pantalla
    public TextMeshProUGUI textoShurikens;

    [Header("Barras de UI")]
    public Image healthBarFill;     // Arrastra tu imagen 'Vida_Relleno' aquí
    public Image postureBarFill;    // Arrastra tu imagen 'Postura_Relleno' aquí

    // Guardaremos los valores máximos para calcular el porcentaje
    private float maxHealth;
    private float maxPosture;
    public BaseEntity playerHealth;

    private void Start()
    {
        // NOTA: Asegúrate de que los nombres coincidan con los de tu BaseEntity
        // Si en tu script se llaman de otra forma (ej. vidaMaxima), cámbialos aquí.
        playerHealth = player.Health;
        if (playerHealth != null)
        {
            maxHealth = playerHealth.maximunHealth; 
            maxPosture = playerHealth.maximumPosture;
        }
    }

    private void Update()
    {
        if (playerHealth != null)
        {
            // El Fill Amount funciona de 0.0 (vacío) a 1.0 (lleno).
            // Si tienes 50 de vida y el máximo es 100... 50/100 = 0.5 (Mitad de la barra)
            healthBarFill.fillAmount = playerHealth.ActualHealth / maxHealth;
            postureBarFill.fillAmount = playerHealth.ActualPosture / maxPosture;
        }
        textoShurikens.text = player.throwableWeaponCounter + "/" + player.throwableWeaponLimit;
    }
}