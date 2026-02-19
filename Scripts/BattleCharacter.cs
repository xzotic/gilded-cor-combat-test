using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class BattleCharacter : MonoBehaviour
{
    public CharacterData data;
    public int level;

    [Header("Base Stats")]
    public int baseHP;
    public int baseSP = 100;
    public int baseMentalRes;
    public int basePhysAtk;
    public int basePhysDef;
    public int baseSpeAtk;
    public int baseSpeDef;

    [Header("EffectiveStats")]
    public int effHP;
    public int effSP;
    public int effMentalRes;
    public int effPhysAtk;
    public int effPhysDef;
    public int effSpeAtk;
    public int effSpeDef;

    [Header("Equipment")]
    public int weaponID;

    [Header("Equipped Skills")]
    public Skill[] skills = new Skill[6];

    [Header("Special Resistances")]
    public float specialRes, flameRes, lightRes, darkRes, fateRes, plagueRes, galeRes, natureRes, deathRes, starRes;

    [Header("Physical Resistances")]
    public float cutRes, bluntRes, pierceRes, physRes;

    private void Awake()
    {
        UpdateLevel();
        ScaleStat();
        SetStat();
    }

    private void OnEnable()
    {
        UpdateLevel();
        ScaleStat();
    }

    private void ScaleStat()
    {
        baseHP = Mathf.RoundToInt(data.baseHP * ( 1 + data.hpGrowthRate * Mathf.Pow(level-1,1.4f )));
        baseMentalRes = data.baseMentalRes;

        basePhysAtk = Mathf.RoundToInt(data.basePhysAttack * ( 1 + data.physAttackGrowthRate * Mathf.Pow(level - 1, 1.1f )));
        baseSpeAtk = Mathf.RoundToInt(data.baseSpeAttack * ( 1 + data.speAttackGrowthRate * Mathf.Pow(level - 1, 1.1f )));

        basePhysDef = Mathf.RoundToInt(data.basePhysDefense * ( 1 + data.physDefenseGrowthRate * Mathf.Pow(level - 1, 1.1f )));
        baseSpeDef = Mathf.RoundToInt(data.baseSpeDefense * ( 1 + data.speDefenseGrowthRate * Mathf.Pow(level - 1, 1.1f )));
    }

    private void SetStat()
    {
        effHP = baseHP;
        effSP = baseSP;
        effMentalRes = baseMentalRes;
        effPhysAtk = basePhysAtk;
        effSpeAtk = baseSpeAtk;
        effSpeDef = baseSpeDef;
        effPhysDef = basePhysDef;
        effSpeDef = baseSpeDef;
    }

    public void UpdateLevel()
    {
        level = this.transform.parent.GetComponent<GlobalData>().level;
    }
}
