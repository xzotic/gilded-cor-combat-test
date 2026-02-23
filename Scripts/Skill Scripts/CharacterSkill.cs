using System;
using System.Collections.Generic;
using Unity.InferenceEngine.Tokenization;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.TextCore.Text;
using Random = UnityEngine.Random;

public abstract class Skill : ScriptableObject
{
    public string skillName;
    public Sprite icon;

    [Header("Attributes")]
    public TargetType target;
    public SkillCategory skillCategory;
    public SkillType skillType;
    public MeleeType meleeType;
    public SorceryType sorceryType;


    public int minRoll;
    public int maxRoll;
    public int basePower;
    public int atkWeight;

    public int cooldown;

    public enum TargetType
    {
        None,
        Self,
        Ally,
        Enemy,
        AllAllies,
        AllEnemies
    }

    public enum SkillCategory
    {
        None, Offense, Recovery, Buff
    }

    public enum SkillType
    {
        None, Sorcery, Melee, Defend
    }

    public enum MeleeType
    {
        None, Sever, Impact, Puncture
    }

    public enum SorceryType
    {
        None, 
        [InspectorName("Intra's Flame")] Flame,
        Flesh,
        Light,
        Dark,
        Fate,
        Plague,
        Gale,
        Nature,
        Death,
        Star
    }


    public abstract void Use(int user, Enemy targetEnemy, BattleCharacter targetAlly, int roll);

    public virtual float CalculateRawDMG(BattleCharacter bc, Enemy enemy, float rollMult)
    {
        float dmg;

        dmg = (((((2 * bc.level) / 100) + 1) * basePower * bc.effSpeAtk * ReturnEnemyRes(enemy)) / 50) * rollMult;

        return dmg;
    }

    private float ReturnEnemyRes(Enemy enemy)
    {
        if (skillType == SkillType.Sorcery)
        {
            switch (sorceryType)
            {
                case SorceryType.Flame: return enemy.flameRes;
                case SorceryType.Light: return enemy.lightRes;
                case SorceryType.Fate: return enemy.fateRes;
                case SorceryType.Plague: return enemy.plagueRes;
                case SorceryType.Gale: return enemy.galeRes;
                case SorceryType.Nature: return enemy.natureRes;
                case SorceryType.Death: return enemy.deathRes;
                case SorceryType.Flesh: return enemy.fleshRes;
                case SorceryType.Star: return enemy.starRes;
                case SorceryType.Dark: return enemy.darkRes;
                default: return 1f;
            }
        }
        else if (skillType == SkillType.Melee)
        {
            switch (meleeType)
            {
                case MeleeType.Sever: return enemy.severRes;
                case MeleeType.Impact: return enemy.impactRes;
                case MeleeType.Puncture: return enemy.punctureRes;
                default: return 1f;
            }
        }
        else return 1f;
    }
}
