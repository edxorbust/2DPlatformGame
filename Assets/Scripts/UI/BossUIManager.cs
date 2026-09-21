using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossUIManager : MonoBehaviour
{
    [Header("Referencias del Jefe")]
    public EnemyController boss;        
    public GameObject bossUIContainer;  

    [Header("Barras de UI")]
    public Image healthBarFill;
    public Image postureBarFill;
    private bool uiActive;
    
    [Header("Sistema de Finisher (Vidas)")]
    public GameObject finisherIconPrefab;    // Arrastra aquí el prefab de tu icono (una Image)
    public Transform finisherIconContainer;  // Arrastra aquí el Panel vacío que contendrá los iconos

    // Lista para guardar y controlar los iconos generados
    private List<GameObject> iconosActivos = new List<GameObject>();

    private float maxHealth;
    private float maxPosture;
    public bool hasFightStarted = false;

    private void Start()
    {

        // Apagamos la interfaz del jefe al inicio del nivel
        if (bossUIContainer != null)
        {
            bossUIContainer.SetActive(false);
        }
    }

    private void Update()
    {
        if (boss == null || boss.Health == null) return;

        // 1. ¿El Jefe ya nos vio y empezó la pelea?
        if (hasFightStarted && !uiActive)
        {
            maxHealth = boss.Health.maximunHealth;
            maxPosture = boss.Health.maximumPosture;
            if (bossUIContainer != null) bossUIContainer.SetActive(true);
            uiActive = true;
            // Generamos los iconos justo cuando empieza el combate
            GenerarIconos(boss.Health.finisherLimit);
        }

        // 2. Si la pelea empezó y el jefe muere, apagamos la barra
        if (hasFightStarted && boss.StateMachine.CurrentState == boss.DeathState)
        {
            if (bossUIContainer != null) bossUIContainer.SetActive(false);
            uiActive = false;
            hasFightStarted = false; 
            return;
        }

        // 3. Actualizar la barra y los iconos durante la pelea
        if (uiActive)
        {
            healthBarFill.fillAmount = boss.Health.ActualHealth / maxHealth;
            
            if (postureBarFill != null) 
                postureBarFill.fillAmount = boss.Health.ActualPosture / maxPosture;
                
            // Comprobamos si el jefe perdió una vida y borramos el icono correspondiente
            ActualizarIconos(boss.Health.finisherLimit); 
        }
    }

    // --- MÉTODOS NUEVOS PARA LOS ICONOS ---

    private void GenerarIconos(int cantidadMaxima)
    {
        // Por seguridad, limpiamos la lista si ya había iconos
        foreach (GameObject icono in iconosActivos)
        {
            Destroy(icono);
        }
        iconosActivos.Clear();

        // Instanciamos un icono por cada vida que tenga el jefe
        for (int i = 0; i < cantidadMaxima; i++)
        {
            GameObject nuevoIcono = Instantiate(finisherIconPrefab, finisherIconContainer, false);
            iconosActivos.Add(nuevoIcono);
        }
    }

    private void ActualizarIconos(int vidasActuales)
    {
        // Mientras haya más iconos en la UI que vidas reales del jefe...
        while (iconosActivos.Count > vidasActuales)
        {
            // Encontramos el índice del último icono (el de más a la derecha)
            int ultimoIndice = iconosActivos.Count - 1;
            
            // Lo destruimos visualmente
            Destroy(iconosActivos[ultimoIndice]);
            
            // Lo sacamos de nuestra lista
            iconosActivos.RemoveAt(ultimoIndice);
        }
    }
}