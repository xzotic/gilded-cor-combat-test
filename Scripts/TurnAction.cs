using System;
using UnityEngine;

public enum ActionType
{
    WeaponMelee,
    SorcerySkill,
    DefendSkill,
    EndTurn,
    PassTurn
}

[Serializable]
public class TurnAction
{
    public int characterIndex; // index in deployment order (0,1,2,3)
    public int characterID;

    public ActionType actionType;
    public Skill.TargetType targetType;

    public BattleCharacter target_ally;
    public Enemy target_enemy;
    public EnemyPart target_part;

    public Skill skill; // sorcery skill
    public string name; // action name
}
