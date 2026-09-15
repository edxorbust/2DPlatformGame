using UnityEngine;
using System;

public class BaseEntity : MonoBehaviour
{
    [Header("Posture")]
    public int maximumPosture = 100;
    public bool isBoss = false; 

    public int ActualPosture { get; private set; }
    public int finisherLimit = 3;
    public int HealPotions = 2;
    public int cureQuantity = 40;
    public float postureRecoverySpeed = 0.002f;
    public float timeUntilPostureRecovery = 2.5f;
    public float actualTimeRemainingforRecovery;
    private float postureRegenBuffer = 0f;
    
    
    public event Action AtBreakPosture;

    [Header("Base Stadistics")]
    [SerializeField] public int maximunHealth = 100;
    public bool IsParrying { get; set; } // Bandera para saber si está en postura de parry
    public bool IsBlocking { get; set; }
    public bool IsDashing { get; set; }
    public bool HasSecondPhase = false;
    public bool InSecondPhase = false;
    public float secondPhaseStatsMultiplier = 1.5f;

    public int ActualHealth { get; private set;}

    public event Action<int> GettingDamage;
    public event Action Dying;
    public event Action DoingParry; // Nuevo evento que grita: "¡Desvié un golpe!"

    protected virtual void Awake()
    {
        ActualHealth = maximunHealth;
        ActualPosture = maximumPosture;
    }

    public virtual void EnviromentDamage(int quantity)
    {
        if (quantity <= 0)
        {
            return;
        }

        if (IsDashing)
        {
            return;
        }

        actualTimeRemainingforRecovery = timeUntilPostureRecovery;

        ActualHealth -= quantity;
        GettingDamage?.Invoke(quantity);

        if(ActualHealth <= 0)
        {
            if (finisherLimit <= 0)
            {
                ActualHealth = 0;
                Die();
            }
            else
            {
                finisherLimit--;
                if (finisherLimit <= 0)
                {
                    Die();
                    return;
                }
                ActualHealth = maximunHealth;
                RestartPosture();
            }
        }
        ActualPosture -= quantity/2;
    }

    public virtual void Damage(int quantity)
    {
        if (quantity <= 0)
        {
            return;
        }

        if (IsParrying)
        {
            Debug.Log($"{gameObject.name} reescribió el tiempo. ¡PARRY EXITOSO!");
            DoingParry?.Invoke();
            return; // El return corta la función aquí; el daño nunca se aplica
        }

        if (IsDashing)
        {
            return;
        }

        actualTimeRemainingforRecovery = timeUntilPostureRecovery;

        ActualHealth -= quantity;

        GettingDamage?.Invoke(quantity);

        if(ActualHealth <= 0)
        {
            if (finisherLimit <= 0)
            {
                ActualHealth = 0;
                Die();
            }
            else
            {
                finisherLimit--;
                if (finisherLimit <= 0)
                {
                    Die();
                    return;
                }
                if (finisherLimit <= 1 && HasSecondPhase)
                {
                    InSecondPhase = true;
                    ActualHealth = maximunHealth * 3;
                    EnemyController enemy = GetComponent<EnemyController>();
                    enemy.attackDamage = (int)(enemy.attackDamage * secondPhaseStatsMultiplier);
                    enemy.StartSecondPhase();
                    RestartPosture();
                    return;
                }
                ActualHealth = maximunHealth;
                RestartPosture();
            }
        }
        ActualPosture -= quantity/2;
    }

    public void PostureDamage(int quantity)
    {
        if (ActualPosture <= 0) return; // Ya está rota
        actualTimeRemainingforRecovery = timeUntilPostureRecovery;
        ActualPosture -= quantity;
        Debug.Log($"{gameObject.name} perdió {quantity} de postura. Postura restante: {ActualPosture}");

        if (ActualPosture <= 0)
        {
            ActualPosture = 0;
            AtBreakPosture?.Invoke(); // Grita: "¡Me rompieron la guardia!"
        }
    }

    public void RestartPosture()
    {
        ActualPosture = maximumPosture;
    }

    public void HealingPosture()
    {
        if (ActualPosture >= maximumPosture)
        {
            // Limpiamos el buffer por si acaso y aseguramos que no pase del máximo
            postureRegenBuffer = 0f;
            ActualPosture = maximumPosture;
            return;
        }

        // 1. Calculamos cuánta postura exacta debemos recuperar en ESTE frame (con decimales)
        // Multiplicamos por Time.deltaTime para que sea independiente de los FPS
        float cantidadEsteFrame = (maximumPosture * postureRecoverySpeed) * Time.deltaTime;
        
        // 2. Lo guardamos en nuestro acumulador invisible
        postureRegenBuffer += cantidadEsteFrame;

        // 3. Si nuestro acumulador ya juntó 1 punto entero (o más)...
        if (postureRegenBuffer >= 1f)
        {
            // Sacamos los puntos enteros que logramos juntar
            int puntosARestaurar = (int)MathF.Floor(postureRegenBuffer);
            
            // Se los damos a la postura real
            ActualPosture += puntosARestaurar;
            
            // Le restamos esos enteros al buffer, pero conservamos los decimales sobrantes
            postureRegenBuffer -= puntosARestaurar; 
        }
    }

    public void FinisherDamage()
    {
        // 1. Le restamos una "vida" o marca de ejecución
        finisherLimit--;
        Debug.Log($"¡FINISHER APLICADO! A {gameObject.name} le quedan {finisherLimit} vidas.");

        // 2. Si ya no le quedan vidas (o era un enemigo común de 1 vida) -> Muere
        if (finisherLimit <= 0)
        {
            ActualHealth = 0;
            Die();
            return;
        }
        if (finisherLimit <= 1 && HasSecondPhase)
        {
            InSecondPhase = true;
            ActualHealth = maximunHealth * 3;
            EnemyController enemy = GetComponent<EnemyController>();
            enemy.attackDamage = (int)(enemy.attackDamage * secondPhaseStatsMultiplier);
            RestartPosture();
            return;
        }
        RestartPosture();
    }

    public virtual void Cure(int quantity)
    {
        ActualHealth += quantity;
        if(ActualHealth > maximunHealth)
        {
            ActualHealth = maximunHealth;
        }
    }

    public virtual void Die()
    {
        Rigidbody2D Rb = GetComponent<Rigidbody2D>();
        Rb.linearVelocity = Vector2.zero;
        Rb.simulated = false; // Apagamos físicas
        Dying?.Invoke();
        Debug.Log("Objeto ha muerto");

    }

    public virtual void Update()
    {
        if (actualTimeRemainingforRecovery > 0f)
        {
            actualTimeRemainingforRecovery -= Time.deltaTime;
            if (actualTimeRemainingforRecovery < 0f)
            {
                actualTimeRemainingforRecovery = 0f;
            }
        }
        if (actualTimeRemainingforRecovery <= 0f && ActualPosture < maximumPosture)
        {
            HealingPosture();
        }
        if (ActualPosture <= 0)
        {
            ActualPosture = 0;
            AtBreakPosture?.Invoke();
        }
    }
}
