using System;
using UnityEngine;

public class Shuriken : MonoBehaviour
{
    [SerializeField] private LayerMask impactLayers;
    private PlayerController player;
    public float velocidad = 5f;
    private Vector2 direccionFinal;
    public float fromDirection;
    public float lifeTime = 4f;

    [Header("Rotación")]
    public float velocidadRotacion = 720f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void StartShuriken(PlayerController entity)
    {
        player = entity;   
        if (player.transform.localScale.x >= 1f)
        {
            direccionFinal = new Vector2(1f, 0f); // Se moverá hacia la derecha
            fromDirection = 1.0f;
        }
        else
        {
            // Si es 180 o -180, mira a la izquierda
            direccionFinal = new Vector2(-1f, 0f); // Se moverá hacia la izquierda
            fromDirection = -1.0f;
        }
        Destroy(gameObject, lifeTime);
    }

    // Update is called once per frame
    void Update()
    {

        //moving
        transform.Translate(direccionFinal * velocidad * Time.deltaTime, Space.World);

        float direccionGiro = direccionFinal.x < 0 ? 1f : -1f; 
        
        transform.Rotate(0f, 0f, velocidadRotacion * direccionGiro * Time.deltaTime);

        //attacking

        Collider2D impact = Physics2D.OverlapCircle(transform.position, 0.4f, impactLayers);
        bool connectedHit = false;

        if(impact != null)
        {
            
            // Buscamos si es un enemigo con cerebro (Base)
            EnemyController enemyController = impact.GetComponent<EnemyController>();
            
            if (enemyController != null)
            {    
                enemyController.GetAttack(player.GetLightAttackDamage(), player.transform, true, fromDirection);
                connectedHit = true;
            }
            else
            {
                // Si no tiene cerebro (ej. una caja rompible), solo le hacemos daño normal
                BaseEntity entity = impact.GetComponent<BaseEntity>();
                if (entity != null)
                {
                    entity.Damage(player.GetLightAttackDamage());
                    connectedHit = true;
                }
                else
                {
                    Destroy(gameObject);
                }
            }
            if (connectedHit)
            {
                player.TriggerHitStop(player.lightHitStopDuration);
                Destroy(gameObject); 
            }
        } 
        
    }
}