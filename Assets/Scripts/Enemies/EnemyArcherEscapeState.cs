using UnityEngine;

public class EnemyArcherEscapeState : BaseState
{
    private ArcherController enemy;

    public EnemyArcherEscapeState(ArcherController enemyContext)
    {
        enemy = enemyContext;
    }

    public override void Enter()
    {
        Debug.Log("entrando a escape state");
    }

    public override void Update()
    {
        if (enemy.playerTarget == null) return;

        float distance = enemy.DistanceToPlayer();

        // 1. Si el jugador se aleja mucho, volvemos a Idle
        if (distance > enemy.detectionRange * 1.5f)
        {
            enemy.StateMachine.ChangeState(enemy.IdleState);
            return;
        }

        // 2. Si estamos a distancia segura, atacamos
        if (distance >= enemy.safeDistance && enemy.CanSeePlayer())
        {
            if (enemy.CanAttack())
            {
                enemy.IniciarSecuenciaAleatoria();
            }
        }
    }

    public override void PhysicsUpdate()
    {
        if (enemy.playerTarget == null) return;

        float distance = enemy.DistanceToPlayer();
        float dirToPlayer = Mathf.Sign(enemy.playerTarget.position.x - enemy.transform.position.x);

        // 3. Huida física
        if (distance < enemy.safeDistance)
        {
            Debug.Log("player demasiado cerca");
            float fleeDir = -dirToPlayer;
            bool canFleeBack = CheckGroundAndWall(fleeDir);
            Debug.Log(canFleeBack);

            if (canFleeBack)
            {
                Debug.Log("esta huyendo");
                enemy.Rb.linearVelocity = new Vector2(fleeDir * enemy.moveSpeed, enemy.Rb.linearVelocity.y);
                FaceDirection(fleeDir);
                ReproducirAnimacionSegura("Run");
            }
            else
            {
                enemy.Rb.linearVelocity = new Vector2(0, enemy.Rb.linearVelocity.y);
                enemy.FacePlayer();
                ReproducirAnimacionSegura("Idle");
                if (enemy.CanAttack()) enemy.IniciarSecuenciaAleatoria();
            }
        }
        else
        {
            enemy.Rb.linearVelocity = new Vector2(0, enemy.Rb.linearVelocity.y);
            enemy.FacePlayer();
            ReproducirAnimacionSegura("Idle");
        }
    }

    public override void Exit()
    {
        enemy.Rb.linearVelocity = new Vector2(0, enemy.Rb.linearVelocity.y);
    }

    private bool CheckGroundAndWall(float directionX)
    {
        Bounds bounds = enemy.Collider.bounds; 
        
        Vector2 edgePos = new Vector2(bounds.center.x + (bounds.extents.x * directionX), bounds.center.y);
        float wallCheckDist = 0.5f; 
        RaycastHit2D wallHit = Physics2D.Raycast(edgePos, new Vector2(directionX, 0), wallCheckDist, enemy.capaSueloHuir);
        Debug.DrawRay(edgePos, new Vector2(directionX * wallCheckDist, 0), Color.red);

        Vector2 floorPos = new Vector2(bounds.center.x + (bounds.extents.x * directionX), bounds.min.y + 0.1f);
        float floorCheckDist = 1.0f;
        RaycastHit2D floorHit = Physics2D.Raycast(floorPos, Vector2.down, floorCheckDist, enemy.capaSueloHuir);
        Debug.DrawRay(floorPos, Vector2.down * floorCheckDist, Color.green);

        return wallHit.collider == null && floorHit.collider != null;
    }

    private void FaceDirection(float dir)
    {
        if (dir > 0 && enemy.transform.localScale.x != 1f)
            enemy.transform.localScale = new Vector3(1f, 1f, 0);
        else if (dir < 0 && enemy.transform.localScale.x != -1f)
            enemy.transform.localScale = new Vector3(-1f, 1f, 0);
    }

    private void ReproducirAnimacionSegura(string nombreAnimacion)
    {
        if (!enemy.Animator.GetCurrentAnimatorStateInfo(0).IsName(nombreAnimacion))
        {
            enemy.Animator.Play(nombreAnimacion);
        }
    }
}