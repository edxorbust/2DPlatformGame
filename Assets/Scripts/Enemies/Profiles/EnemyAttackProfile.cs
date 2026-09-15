using System.Collections.Generic;
using UnityEngine;

// 1. Las 6 categorías que definiste
public enum AttackCategory 
{ 
    Melee,
    Heavy, 
    Dash, 
    Jump, 
    Shoot, 
    Cast, 
    Finisher 
}

// 2. Una secuencia de ataques (Ej: Salto -> Melee -> Dash)
[System.Serializable]
public struct AttackSequence
{
    public string sequenceName; // Ej: "Combo Agresivo"
    public List<AttackCategory> moves; // La lista de ataques en orden
}

// 3. ¡El Creador de Tarjetas!
[CreateAssetMenu(fileName = "NewEnemyProfile", menuName = "Juego/Perfil de Enemigo")]
public class EnemyAttackProfile : ScriptableObject
{
    [Header("Secuencias Disponibles")]
    public List<AttackSequence> attackSequences;
}