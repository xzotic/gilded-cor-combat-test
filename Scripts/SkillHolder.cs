using UnityEngine;
using UnityEngine.UI;

public class SkillHolder : MonoBehaviour
{
    public int skillIndex;
    public Skill heldSkill;
    public ActionType actionType;
    [SerializeField] private BattleManager battleManager;

    public void SendActionSignal()
    {
        battleManager.SortSkillCategory(heldSkill, skillIndex, actionType);
    }
}