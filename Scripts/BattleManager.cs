using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleManager : MonoBehaviour
{
    public PartyManager partyManager;
    public TurnManager turnManager;
    public GameObject playables;
    public CombatMenuNav menuNav;
    [SerializeField] private GlobalData globalData;
    public int currentTurnIndex;

    [Header("Turn Actions")]
    public List<TurnAction> turnActions;

    [Header("Dice Roll")]
    //public int twodice_1;
    //public int twodice_2;
    public int onedice;
    public int currentRoll;
    public GameObject diceHolder;
    public bool diceStopFlag = false;

    [Header("Enemy")]
    public GameObject enemyHolder;
    public Enemy enemy;
    public EnemyPart EnemyPart;
    public EnemySelector enemySelector;
    public EnemyPartCycle partSelector;

    public bool enemySelectStopFlag = false;
    public bool partSelectStopFlag = false;
    private bool currentlySelecting;

    private void OnEnable()
    {
        diceHolder.SetActive(false);
    }

    public IEnumerator RegisterActions(int index)
    {
        TurnAction action = turnActions[index];
        Debug.Log("Character " + action.characterIndex + " used " + action.name + ", action type: " + action.actionType);

        switch ((int)globalData.rollSetting)  //get roll
        {
            case 0: // manual
                if (action.actionType == ActionType.SorcerySkill) yield return StartCoroutine(RollManual(action.skill.minRoll, action.skill.maxRoll));
                break;
            case 1: // auto
                if (action.actionType == ActionType.SorcerySkill) yield return StartCoroutine(RollAuto(action.skill.minRoll, action.skill.maxRoll));
                break;
            case 2: // Take 10
                if (action.actionType == ActionType.SorcerySkill) yield return StartCoroutine(Roll10(action.skill.minRoll,action.skill.maxRoll));
                break;
        }

        diceHolder.GetComponentInChildren<Button>().interactable = false;

        switch (action.actionType)
        {
            case ActionType.WeaponMelee:
                break;
            case ActionType.SorcerySkill:
                action.skill.Use(action.characterID, action.target_enemy, action.target_ally, currentRoll);
                break;
            case ActionType.DefendSkill:
                break;
            case ActionType.EndTurn:
                break;
            case ActionType.PassTurn:
                break;
        }

        diceStopFlag = false;
        yield return new WaitForSeconds(1f);
    }

    public void EnterToActionQueue(int index, ActionType actionType)    // todo: remove ActionType actionType
    {
        EnterToActionQueue(index, actionType, null, null);
    }

    public void EnterToActionQueue(int index, ActionType actionType, Enemy selectedEnemy, EnemyPart selectedPart)
    {
        TurnAction turnAction = new TurnAction();
        turnAction.characterIndex = turnManager.currentPlayerIndex; //0,1,2,3
        turnAction.characterID = turnManager.characterIndex;

        if (index != -1) // Sorcery Skill
        {
            BattleCharacter bc = partyManager.GetBattleCharacter(turnManager.characterIndex);
            //bc.skills[index].Use();
            //Debug.Log(bc.skills[index]);

            turnAction.actionType = actionType;
            turnAction.skill = bc.skills[index];
            turnAction.name = bc.skills[index].skillName;

            Skill.TargetType target = turnAction.skill.target;


            switch (target)
            {
                case Skill.TargetType.Ally:
                    break;
                case Skill.TargetType.Enemy:
                    turnAction.targetType = Skill.TargetType.Enemy;

                    turnAction.target_ally = partyManager.GetBattleCharacter(turnAction.characterID);
                    turnAction.target_enemy = selectedEnemy;
                    turnAction.target_part = selectedPart;


                    break;
                case Skill.TargetType.AllAllies:
                    break;
                case Skill.TargetType.AllEnemies:
                    break;
                case Skill.TargetType.Self:
                    turnAction.targetType = Skill.TargetType.Self;

                    turnAction.target_ally = partyManager.GetBattleCharacter(turnAction.characterID);
                    turnAction.target_enemy = null;
                    break;
            }

            turnActions.Add(turnAction);
            return;

        } else // not sorcery skill
        {
            if (actionType == ActionType.EndTurn)
            {
                turnAction.actionType = actionType;
                turnAction.skill = null;
                turnAction.name = "End Turn";
                turnAction.target_enemy = null;
                turnAction.target_part = null;

                turnActions.Add(turnAction);
                return;
            }
        }
    }

    public void EnterEndTurnActionToQueue()
    {
        EnterToActionQueue(-1, ActionType.EndTurn);
    }

    public void SelectAndEnterAction(int index, ActionType actionType)
    {
        StartCoroutine(SelectAndEnterActionRoutine(index, actionType));
    }

    private IEnumerator SelectAndEnterActionRoutine(int index, ActionType actionType)
    {
        if (index == -1 || actionType != ActionType.SorcerySkill)
        {
            EnterToActionQueue(index, actionType);
            yield break;
        }

        BattleCharacter bc = partyManager.GetBattleCharacter(turnManager.characterIndex);
        Skill selectedSkill = bc.skills[index];

        if (selectedSkill.target != Skill.TargetType.Enemy)
        {
            EnterToActionQueue(index, actionType);
            yield break;
        }

        Enemy selectedEnemy = null;
        EnemyPart selectedPart = null;

        Enemy[] activeEnemies = GetActiveEnemies();
        if (activeEnemies.Length == 0)
        {
            Debug.LogWarning("No enemy found for enemy-targeted skill.");
            yield break;
        }

        if (activeEnemies.Length == 1)
        {
            selectedEnemy = activeEnemies[0];
        }
        else
        {
            yield return StartCoroutine(SelectEnemyEnum(activeEnemies));
            selectedEnemy = enemySelector != null ? enemySelector.selectedEnemy : null;

            if (selectedEnemy == null)
            {
                yield break;
            }
        }

        yield return StartCoroutine(SelectTargetEnum(selectedEnemy));
        if (selectedEnemy.currentSelectedPart != null)
        {
            selectedPart = selectedEnemy.currentSelectedPart.GetComponent<EnemyPart>();
        }

        if (selectedPart == null)
        {
            yield break;
        }

        EnterToActionQueue(index, actionType, selectedEnemy, selectedPart);
    }

    private Enemy[] GetActiveEnemies()
    {
        if (enemyHolder == null)
        {
            return new Enemy[0];
        }

        return enemyHolder.GetComponentsInChildren<Enemy>();
    }

    private IEnumerator SelectEnemyEnum(Enemy[] activeEnemies)
    {
        currentlySelecting = true;

        menuNav.DisplayDice(false);
        menuNav.DisplayMainLayer();
        menuNav.DisableMainLayer();

        enemySelectStopFlag = false;

        if (enemySelector == null)
        {
            enemySelector = enemyHolder.GetComponentInChildren<EnemySelector>(true);
        }

        if (enemySelector == null)
        {
            Debug.LogWarning("EnemySelector is not assigned.");
            currentlySelecting = false;
            yield break;
        }

        enemySelector.gameObject.SetActive(true);
        enemySelector.StartSelection(activeEnemies, this);

        yield return new WaitUntil(() => enemySelectStopFlag == true);

        currentlySelecting = false;
    }


    private IEnumerator SelectTargetEnum(Enemy selectedEnemy)
    {
        currentlySelecting = true;

        menuNav.DisplayDice(false);
        menuNav.DisplayMainLayer();
        menuNav.DisableMainLayer();

        if (partSelector == null)
        {
            partSelector = enemyHolder.GetComponentInChildren<EnemyPartCycle>(true);
        }

        if (partSelector == null)
        {
            Debug.LogWarning("EnemyPartCycle is not assigned.");
            currentlySelecting = false;
            yield break;
        }

        partSelector.gameObject.SetActive(true);
        partSelector.StartSelection(selectedEnemy);

        yield return new WaitUntil(() => partSelectStopFlag == true);

        currentlySelecting = false;
    }


    public IEnumerator RollManual(int minRoll, int maxRoll)
    {
        diceHolder.SetActive(true);
        diceHolder.GetComponentInChildren<Button>().interactable = true;

        diceStopFlag = false;
        currentRoll = minRoll;

        while (!diceStopFlag)
        {
            currentRoll = Random.Range(minRoll, maxRoll + 1); // inclusive
            diceHolder.GetComponentInChildren<TextMeshProUGUI>().text = currentRoll.ToString();

            yield return new WaitForSeconds(0.03f);
        }
    }

    public IEnumerator RollAuto(int minRoll, int maxRoll)
    {
        diceHolder.SetActive(true);
        diceHolder.GetComponentInChildren<Button>().interactable = false;

        float timer = 0f;
        float duration = 0.5f;

        diceStopFlag = false;
        currentRoll = minRoll;

        while (timer < duration)
        {
            currentRoll = Random.Range(minRoll, maxRoll + 1);
            diceHolder.GetComponentInChildren<TextMeshProUGUI>().text = currentRoll.ToString();

            timer += 0.01f;
            yield return new WaitForSeconds(0.01f);
        }

        diceStopFlag = true;
        yield return new WaitUntil(() => diceStopFlag == true);
    }

    public IEnumerator Roll10(int minRoll, int maxRoll)
    {
        diceHolder.SetActive(true);
        diceHolder.GetComponentInChildren<Button>().interactable = false;

        float timer = 0f;
        float duration = 0.2f;

        diceStopFlag = false;
        currentRoll = minRoll;

        while (timer < duration)
        {
            currentRoll = Random.Range(minRoll, maxRoll + 1);
            diceHolder.GetComponentInChildren<TextMeshProUGUI>().text = currentRoll.ToString();

            timer += 0.01f;
            yield return new WaitForSeconds(0.01f);
        }
        currentRoll = (maxRoll + minRoll) / 2;
        diceHolder.GetComponentInChildren<TextMeshProUGUI>().text = currentRoll.ToString();

        diceStopFlag = true;
        yield return new WaitUntil(() => diceStopFlag == true);
    }

    public void TriggerDiceStopFlag()
    {
        diceStopFlag = true;
    }
}
