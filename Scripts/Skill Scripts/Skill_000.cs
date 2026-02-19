using UnityEngine;
using UnityEngine.TextCore.Text;

[CreateAssetMenu(menuName = "RPG/Skills/Damage Skill")]
public class Skill_000 : Skill
{
    // Test Damage Skill

    public override void Use(int user, Enemy targetEnemy, BattleCharacter targetAlly, int roll)
    {
        float rollMult = ((float)roll) / 20f;
        int outputDMG = Mathf.RoundToInt(CalculateRawDMG(targetAlly, targetEnemy, rollMult));

        int hp = Mathf.Clamp(targetEnemy.currentHP - outputDMG, 0, targetEnemy.maxHP);
        targetEnemy.currentHP = hp;

        Debug.Log("<color=cyan>Character #"+user+ " dealt <color=red>" + outputDMG + "<color=cyan> damage to <color=red>[[" + targetEnemy.enemyName + "]]<color=cyan> with " + skillName + ", rolled " + roll + "</color></color>");
    }
}
