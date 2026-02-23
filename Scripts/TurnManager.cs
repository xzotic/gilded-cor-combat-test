using UnityEngine;
using System.Collections;

public class TurnManager : MonoBehaviour
{
    public enum BattleState
    {
        PlayerPhase,
        EnemyPhase,
        Busy
    }

    public BattleState state;

    public int currentPlayerIndex; // 0,1,2,3
    public int characterIndex; // character id

    [SerializeField] private PartyManager partyManager;
    [SerializeField] private BattleManager battleManager;
    [SerializeField] CombatMenuNav menuNav;

    //[SerializeField] private GameObject skillButtonPrefab;
    //[SerializeField] private GameObject attackMenuLayer;

    private void Start()
    {
        foreach (Transform t in this.transform)
        {
            t.gameObject.SetActive(false);
        }

        StartPlayerPhase();
    }

    void StartPlayerPhase()
    {

        menuNav.DisplayMainLayer();
        state = BattleState.PlayerPhase;
        currentPlayerIndex = 0;

        // Smoothly reset portraits at START of player phase
        partyManager.AnimateResetToOriginalOrder();

        GlobalData globalData = this.gameObject.GetComponent<GlobalData>();
        battleManager.turnActions.Clear();

        Debug.Log("<color=yellow>Started Player Phase</color>");
        StartTurn();
    }

    void StartTurn()
    {
        menuNav.DisplayMainLayer();
        characterIndex = partyManager.activeMembers[currentPlayerIndex];
        partyManager.EnableCharacterTurn(characterIndex);
        partyManager.UpdateHPSP(characterIndex);
    }

    // Finish Turn

    public void FinishTurn()
    {
        if (state != BattleState.PlayerPhase)
            return;

        state = BattleState.Busy;

        characterIndex = partyManager.activeMembers[currentPlayerIndex];
        partyManager.EndCharacterTurn(characterIndex);

        // Last player?
        if (currentPlayerIndex >= partyManager.activeMembers.Length - 1)
        {
            EndPlayerPhase();
            return;
        }

        currentPlayerIndex++;
        partyManager.NextTurn();

        state = BattleState.PlayerPhase;
        StartTurn();
    }

    private IEnumerator RegisterPlayerActions()
    {
        Debug.Log("<color=yellow>Registering player actions</color>");
        state = BattleState.Busy;

        partyManager.AnimateResetToOriginalOrder();

        yield return StartCoroutine(battleManager.RegisterActions(0));
        battleManager.diceHolder.SetActive(false);

        int count = partyManager.activeMembers.Length;

        for (int i = 0; i < count-1; i++)
        {
            partyManager.NextTurn();
            yield return StartCoroutine(battleManager.RegisterActions(i+1));
            battleManager.diceHolder.SetActive(false);
        }

        StartEnemyPhase();
    }

    /* ===============================
       ENEMY PHASE
       =============================== */

    void EndPlayerPhase()
    {
        Debug.Log("<color=yellow>Ended Player Phase</color>");
        menuNav.DisableMainLayer();
        StartPlayerActionRegisterPhase();
    }

    void StartEnemyPhase()
    {
        menuNav.DisplayDice(false);

        state = BattleState.EnemyPhase;
        Debug.Log("<color=yellow>Started Enemy Phase</color>");

        Invoke(nameof(EndEnemyPhase), 2f);
    }

    void EndEnemyPhase()
    {
        Debug.Log("<color=yellow>Ended Enemy Phase</color>");
        StartPlayerPhase();
    }

    void StartPlayerActionRegisterPhase()
    {
        menuNav.DisplayDice(true);
        StartCoroutine(RegisterPlayerActions());

    }

    public Skill ReturnActiveSkill(int index)
    {
        BattleCharacter bc = partyManager.GetBattleCharacter(characterIndex);
        if (index <= bc.skills.Length - 1)
        {
            return bc.skills[index];
        }
        else return null;
    }
}
