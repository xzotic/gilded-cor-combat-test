using UnityEngine;

public abstract class EnemySkill : ScriptableObject
{
    public int thresholdHP;
    public int weight;

    public enum enemyTargetType
    {
        Self,
        Ally,
        Enemy,
        AllAllies,
        AllEnemies
    }

    public abstract void Use(BattleCharacter target);
}
