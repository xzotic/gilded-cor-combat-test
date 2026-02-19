using UnityEngine;

public class SkillHolder : MonoBehaviour
{
    public int skillIndex;
    public ActionType actionType;
    [SerializeField] private TurnManager turnManager;
    [SerializeField] private BattleManager battleManager;

    public void EnterToActionQueue()
    {
        battleManager.EnterToActionQueue(skillIndex, actionType);
    }
}
