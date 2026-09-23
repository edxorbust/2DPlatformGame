using Unity.VisualScripting;
using UnityEngine;

public class ArcherController : EnemyController
{
    [Header("Archer Specifics")]
    public GameObject arrowPrefab;
    public Transform shootPoint;
    public float arrowSpeed = 10f;
    [Tooltip("Distancia en Y para separar las flechas arriba y abajo en el Heavy Attack")]
    public float heavyAttackHeightOffset = 0.5f;
    private int arrowsCount = 0;

    [Header("Comportamiento a Distancia")]
    public float safeDistance = 4f; 
    public LayerMask capaSueloHuir; 

    [Header("Comportamiento")]
    public EnemyAttackProfile attackProfile; 
    private AttackSequence currentSequence; 
    private int currentMoveIndex = 0; 

    [Header("Defenses (Mínima Probabilidad)")]
    [Range(0f, 1f)] public float parryChance = 0.05f; 
    [Range(0f, 1f)] public float blockChance = 0.10f; 
    public int postureDamageOnParried = 20; 

    [Header("Visual & Audio Exclusivos")]
    public GameObject hitSparkParryPrefab; 
    public AudioClip parrySound; 
    public AudioClip blockSound; 
    public AudioClip shootSound;

    public EnemyBlockState BlockState { get; private set; } 
    public EnemyShootState ShootState { get; private set; } 
    public EnemyHeavyShootState HeavyShootState { get; private set; } 
    public EnemyArcherEscapeState EscapeState { get; private set; }

    protected override void Awake()
    {
        base.Awake(); 
        BlockState = new EnemyBlockState(this); 
        ShootState = new EnemyShootState(this); 
        HeavyShootState = new EnemyHeavyShootState(this);
        EscapeState = new EnemyArcherEscapeState(this);

        isRangeEnemy = true; 
    }

    public override void IniciarAtaque()
    {
        // Cuando el ChaseState llame a "IniciarAtaque", el Swordsman
        // ignorará el código base y ejecutará su propia lógica compleja:
        IniciarSecuenciaAleatoria();
    }


    public override bool IntentarAtaqueLargo()
    {
        return false;
    }

    public void IniciarSecuenciaAleatoria()
    {
        int indexAleatorio = Random.Range(0, attackProfile.attackSequences.Count); 
        currentSequence = attackProfile.attackSequences[indexAleatorio]; 
        currentMoveIndex = 0; 
        EjecutarSiguienteMovimiento(); 
    }

    public override void EjecutarSiguienteMovimiento()
    {
        if (currentMoveIndex >= currentSequence.moves.Count || !CanSeePlayer()) 
        {
            ResetAttackCooldown(); 
            StateMachine.ChangeState(IdleState); 
            return; 
        }

        AttackCategory nextMove = currentSequence.moves[currentMoveIndex]; 
        currentMoveIndex++; 

        switch (nextMove) 
        {
            case AttackCategory.Melee: 
            case AttackCategory.Shoot: 
                StateMachine.ChangeState(ShootState); 
                break;
            case AttackCategory.Cast: 
                StateMachine.ChangeState(HeavyShootState); 
                break;
        }
    }

    public bool CanDefend()
    {
        return StateMachine.CurrentState == IdleState || StateMachine.CurrentState == ChaseState; 
    }

    public override void GetAttack(int quantityDamage, Transform attacker, bool isThrowAttack = false, float directionThrowAttack = 1.0f)
    {
        if (inmuneToAttacks)
        {
            return;
        }
        if (isBlocking) 
        {
            ReproducirSonido(blockSound); 
            Animator.Play("Block"); 
            Health.PostureDamage(postureDamageOnParried / 4); 
            if (Rb != null) 
            {
                float pushDirection = (attacker.position.x > transform.position.x) ? -1f : 1f; 
                Rb.linearVelocity = new Vector2(pushDirection * selfBlockKnockbackForce, Rb.linearVelocity.y); 
            }
            return; 
        }

        if (CanDefend()) 
        {
            float tiradaDados = Random.value; 

            if (tiradaDados <= parryChance && !isThrowAttack) 
            {
                FacePlayer();
                ReproducirSonido(parrySound); 
                BaseEntity saludAtacante = attacker.GetComponent<BaseEntity>(); 
                if (saludAtacante != null) saludAtacante.PostureDamage(attackDamage / 2); 
                
                if (hitSparkParryPrefab != null) Instantiate(hitSparkParryPrefab, blockPoint.position, Quaternion.identity); 
                if (CameraController.Instancia != null) CameraController.Instancia.AgitarCamara(0.1f, 0.04f); 
                
                Animator.Play("Block"); 
                return; 
            }
            else if (tiradaDados <= (parryChance + blockChance)) 
            {
                FacePlayer();
                ReproducirSonido(blockSound); 
                Health.PostureDamage(postureDamageOnParried / 4); 
                StateMachine.ChangeState(BlockState); 
                
                if (Rb != null) 
                {
                    float pushDirection = isThrowAttack ? directionThrowAttack : ((attacker.position.x > transform.position.x) ? -1f : 1f); 
                    Rb.linearVelocity = new Vector2(pushDirection * selfBlockKnockbackForce, Rb.linearVelocity.y); 
                }
                return; 
            }
        }

        base.GetAttack(quantityDamage, attacker); 
    }

    // --- INSTANCIACIÓN DE FLECHAS AJUSTADA POR ALTURA ---
    public override void AnimationEvent_OnImpact() 
    {
        if (arrowPrefab == null || shootPoint == null) return;
        
        if (shootSound != null) ReproducirSonido(shootSound); 

        // Dirección estrictamente horizontal
        float direccionX = (transform.localScale.x == -1f) ? -1f : 1f;
        Vector2 baseDirection = new Vector2(direccionX, 0f).normalized;

        if (StateMachine.CurrentState == HeavyShootState)
        {
            // Tomamos la posición base de disparo
            Vector2 posicionSalida = shootPoint.position;

            // Modificamos únicamente la altura dependiendo de qué flecha toca
            switch (arrowsCount)
            {
                case 0: // Arriba
                    posicionSalida.y += heavyAttackHeightOffset;
                    break;
                case 1: // Abajo
                    posicionSalida.y -= heavyAttackHeightOffset;
                    break;
                case 2: // Centro
                    // Mantiene la posición original de shootPoint
                    break;
            }

            // Disparamos usando la nueva posición
            DispararFlecha(baseDirection, posicionSalida);
            arrowsCount++;

            if (arrowsCount >= 3) arrowsCount = 0;
        }
        else 
        {
            // Ataque ligero: dispara al centro
            DispararFlecha(baseDirection, shootPoint.position);
        }

        if (StateMachine.CurrentState == ShootState) ShootState.OnAnimationImpactFrame();
        else if (StateMachine.CurrentState == HeavyShootState) HeavyShootState.OnAnimationImpactFrame();
    }

    public override void AnimationEvent_OnEnd() 
    {
        arrowsCount = 0;
        StateMachine.CurrentState.AnimationFinishTrigger();
        EjecutarSiguienteMovimiento();
    }

    // Método actualizado para recibir la posición exacta de salida
    private void DispararFlecha(Vector2 direccion, Vector2 posicionSalida)
    {
        GameObject arrow = Instantiate(arrowPrefab, posicionSalida, Quaternion.identity);
        
        ArrowProjectile arrowScript = arrow.GetComponent<ArrowProjectile>();
        if (arrowScript != null)
        {
            arrowScript.ConfigurarFlecha(direccion, arrowSpeed, transform);
        }
    }
}