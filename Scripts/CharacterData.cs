using UnityEngine;
using System.Collections.Generic;

// character asset scriptable object

[CreateAssetMenu(menuName = "RPG/Character")]
public class CharacterData : ScriptableObject
{
    public string characterId;
    public string displayName;

    [Header("Base Stats")]
    public int baseHP;
    public int baseMentalRes;

    public int basePhysAttack;
    public int baseSpeAttack;

    public int basePhysDefense;
    public int baseSpeDefense;


    [Header("Growth Per Level")]
    public float hpGrowthRate;
    //public float mentalResGrowthRate;

    public float physAttackGrowthRate;
    public float speAttackGrowthRate;

    public float physDefenseGrowthRate;
    public float speDefenseGrowthRate;

}