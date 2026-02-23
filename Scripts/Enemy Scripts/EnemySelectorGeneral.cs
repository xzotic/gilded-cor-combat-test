using UnityEngine;

public class EnemySelectorGeneral : MonoBehaviour
{
    public Enemy selectedEnemy;
    public EnemyPart selectedEnemyPart;

    [HideInInspector] public int skillIndex; public ActionType actionType;
    [SerializeField] private TurnManager turnManager;
    [SerializeField] private BattleManager battleManager;
    [SerializeField] private CombatMenuNav menuNav;

    public void FinishTurn()
    {
        turnManager.FinishTurn();
    }

    public void DisplayMain()
    {
        menuNav.DisplayMainLayer();
    }

    public void DisableMain()
    {
        menuNav.DisplayMainLayer();
        menuNav.DisableMainLayer();
    }

    public void EnterToActionQueue()
    {
        battleManager.EnterToActionQueue(skillIndex, actionType, selectedEnemyPart);
        selectedEnemyPart = null;
        DisplayMain();
        FinishTurn();
    }
}
