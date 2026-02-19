using UnityEngine;

[CreateAssetMenu(menuName = "RPG/Skills/Heal Skill")]
public class Skill_001 : Skill
{
    // test self heal skill

    public override void Use(int user, Enemy targetEnemy, BattleCharacter targetAlly, int roll)
    {
        int hp = Mathf.Clamp(targetAlly.effHP + 50, 0, targetAlly.baseHP);
        targetAlly.effHP = hp;
        Debug.Log("<color=green> Healed " + targetAlly.data.displayName + " (Self) for 50 HP</color>");
    }
}
