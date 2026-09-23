using UnityEngine;

public class SwordsmanController : EnemyController
{
    [Header("Swordsman Specifics")]
    public int postureDamageOnParried = 20; 
    public float dashForce = 15f; // Fuerza específica para este enemigo

    [Header("Comportamiento")]
    public EnemyAttackProfile attackProfile; // ¡Aquí arrastras la tarjeta!
    private AttackSequence currentSequence;
    private int currentMoveIndex = 0;

    [Header("Defenses")]
    [Range(0f, 1f)] public float parryChance = 0.10f; 
    [Range(0f, 1f)] public float blockChance = 0.90f; 

    [Header("Visual & Audio Exclusivos")]
    public GameObject hitSparkParryPrefab; 
    public AudioClip parrySound;
    public AudioClip blockSound;
    public GameObject undodgableAttackSign;
    public Transform warningsPosition;

    // ESTADO EXCLUSIVO DE ESTE ENEMIGO
    public EnemyBlockState BlockState { get; private set; }
    public EnemyDashAttackState DashState { get; private set; }
    public EnemyJumpAttackState JumpAttackState { get; private set; }

    protected override void Awake()
    {
        base.Awake(); // Llama al Awake del padre para crear la StateMachine y estados base
        
        // Inicializamos el estado de bloqueo específico de este hijo
        BlockState = new EnemyBlockState(this); 
        DashState = new EnemyDashAttackState(this);
        JumpAttackState = new EnemyJumpAttackState(this);
    }

    // Método exclusivo de defensa
    public bool CanDefend()
    {
        return StateMachine.CurrentState == IdleState || StateMachine.CurrentState == ChaseState;
    }
    public override void AnimationEvent_OnImpact()
    {
        if (StateMachine.CurrentState == AttackState) AttackState.OnAnimationImpactFrame();
        else if (StateMachine.CurrentState == DashState) DashState.OnAnimationImpactFrame();
        else if (StateMachine.CurrentState == HeavyAttackState) HeavyAttackState.OnAnimationImpactFrame();
        else if (StateMachine.CurrentState == JumpAttackState) JumpAttackState.OnAnimationImpactFrame();
    }
    public override void IniciarAtaque()
    {
        // Cuando el ChaseState llame a "IniciarAtaque", el Swordsman
        // ignorará el código base y ejecutará su propia lógica compleja:
        IniciarSecuenciaAleatoria();
    }

    public override void AnimationEvent_OnEnd()
    {
        base.AnimationEvent_OnEnd();
        if (StateMachine.CurrentState == JumpAttackState) JumpAttackState.AnimationFinishTrigger();
    }

    public override bool IntentarAtaqueLargo()
    {
        // Elegimos una secuencia al azar (como ya hacías)
        int indexAleatorio = Random.Range(0, attackProfile.attackSequences.Count);
        AttackSequence posibleSecuencia = attackProfile.attackSequences[indexAleatorio];

        // Miramos de qué tipo es el PRIMER movimiento de ese combo
        AttackCategory primerMovimiento = posibleSecuencia.moves[0];

        // Si es un ataque que sirve de lejos (Dash, Shoot, Cast)
        if (primerMovimiento == AttackCategory.Dash || 
            primerMovimiento == AttackCategory.Shoot || 
            primerMovimiento == AttackCategory.Cast ||
            primerMovimiento == AttackCategory.Jump)
            
        {
            // Guardamos esta secuencia y empezamos el ataque
            currentSequence = posibleSecuencia;
            currentMoveIndex = 0;
            EjecutarSiguienteMovimiento();
            return true; // ¡Aviso de que sí atacamos!
        }

        // Si el primer movimiento era Melee, rechazamos el ataque
        return false; // ¡Aviso de que preferimos caminar!
    }

    public void IniciarSecuenciaAleatoria()
    {
        // Elige un combo al azar de la tarjeta
        int indexAleatorio = Random.Range(0, attackProfile.attackSequences.Count);
        currentSequence = attackProfile.attackSequences[indexAleatorio];
        
        currentMoveIndex = 0; // Empezamos desde el primer movimiento
        EjecutarSiguienteMovimiento();
    }

    // Este es el "Router" que traduce la tarjeta a un State real
    public override void EjecutarSiguienteMovimiento()
    {
        // ¿Ya terminamos el combo? Volvemos a perseguir o descansar
        if (currentMoveIndex >= currentSequence.moves.Count)
        {
            ResetAttackCooldown();
            StateMachine.ChangeState(IdleState);
            return;
        }

        // Leemos qué toca hacer ahora
        AttackCategory nextMove = currentSequence.moves[currentMoveIndex];
        if (isAttackingFar && nextMove != AttackCategory.Dash)
        {
            currentMoveIndex = currentSequence.moves.Count;
            return;
        }
        currentMoveIndex++; // Preparamos el índice para el siguiente turno

        // Mandamos a la máquina al estado correspondiente
        switch (nextMove)
        {
            case AttackCategory.Melee:
                StateMachine.ChangeState(AttackState);
                break;
            case AttackCategory.Dash:
                StateMachine.ChangeState(DashState);
                break;
            case AttackCategory.Jump:
                if (!Health.InSecondPhase)
                {
                    StateMachine.ChangeState(HeavyAttackState);
                    break;
                }
                StateMachine.ChangeState(JumpAttackState);
                break;
            case AttackCategory.Heavy:
                StateMachine.ChangeState(HeavyAttackState);
                break;    
            // ... el resto de tus categorías
        }
    }

    // OVERRIDE: Interceptamos el ataque ANTES de que el padre quite vida
    public override void GetAttack(int quantityDamage, Transform attacker, bool isThrowAttack=false, float directionThrowAttack=1.0f)
    {
        if (inmuneToAttacks)
        {
            return;
        }
        if (isBlocking)
        {
            ReproducirSonido(blockSound);
            Animator.Play("Parry");    
            Health.PostureDamage(postureDamageOnParried/4);
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

            if (tiradaDados <= parryChance && !isThrowAttack) // PARRY
            {
                ReproducirSonido(parrySound);
                BaseEntity saludAtacante = attacker.GetComponent<BaseEntity>();
                if (saludAtacante != null) saludAtacante.PostureDamage(attackDamage/2); 
                
                if (hitSparkParryPrefab != null)
                {
                    Instantiate(hitSparkParryPrefab, blockPoint.position, Quaternion.identity);
                }
                
                if (CameraController.Instancia != null) CameraController.Instancia.AgitarCamara(0.1f, 0.04f);
                Animator.Play("Parry");
                return; 
            }
            else if (tiradaDados <= (parryChance + blockChance)) // BLOQUEO
            {
                ReproducirSonido(blockSound);
                Health.PostureDamage(postureDamageOnParried/4);
                StateMachine.ChangeState(BlockState);
                
                if (Rb != null)
                {
                    if (isThrowAttack)
                    {
                        Rb.linearVelocity = new Vector2(directionThrowAttack * selfBlockKnockbackForce, Rb.linearVelocity.y);
                    }
                    else
                    {
                        float pushDirection = (attacker.position.x > transform.position.x) ? -1f : 1f;
                        Rb.linearVelocity = new Vector2(pushDirection * selfBlockKnockbackForce, Rb.linearVelocity.y);
                    }
                }
                return;
            }
        }

        // Si falló el parry y el bloqueo, llamamos al método del padre para que reciba el daño normal
        base.GetAttack(quantityDamage, attacker);
    }
}