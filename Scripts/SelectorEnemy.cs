using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class SelectorEnemy : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    private InputActionMap actionMap;
    private InputAction action;
    private InputAction action_press;
    private InputAction action_release;
    private InputAction action_mouse;

    [HideInInspector] public int skillIndex; public ActionType actionType;
    [SerializeField] private TurnManager turnManager;
    [SerializeField] private BattleManager battleManager;
    [SerializeField] private CombatMenuNav menuNav;

    public GameObject enemyHolder;
    public List<Enemy> activeEnemies = new List<Enemy>();
    public GameObject currentSelection;

    //public int currentSelectionIndex;
    //public int maxSelectionIndex;

    public Enemy enemy;
    public EnemyPart part;
    public SelectionStage selectionStage;
    public enum SelectionStage
    {
        None, Enemy, Part
    }


    private void OnEnable()
    {
        GetComponent<SpriteRenderer>().color = Color.clear;
        playerInput = GetComponent<PlayerInput>();
        actionMap = playerInput.actions.FindActionMap("Combat Menu");
        action = actionMap.FindAction("Navigation");
        action_press = actionMap.FindAction("Submit");
        action_release = actionMap.FindAction("Cancel");
        action_mouse = actionMap.FindAction("Press");
        selectionStage = SelectionStage.None;

        actionMap.Disable();
    }



    public void StartEnemySelection()
    {
        selectionStage = SelectionStage.Enemy;

        menuNav.DisplayMainLayer();
        menuNav.DisableMainLayer();
        actionMap.Enable();


        foreach (Transform child in enemyHolder.transform)
        {
            if (!child.gameObject.activeInHierarchy)
                continue;

            Enemy enemy = child.GetComponent<Enemy>();

            if (enemy != null)
            {
                activeEnemies.Add(enemy);
            }
        }

        GetComponent<SpriteRenderer>().color = Color.red;
        //currentSelection = activeEnemies[0].gameObject;
        //currentSelectionIndex = 0; maxSelectionIndex = activeEnemies.Count-1;
        transform.position = currentSelection.transform.position;


        action.performed += OnNavigate;
        action_press.performed += FinishEnemySelection;
        action_release.performed += CancelEnemySelection;
    }

    public void FinishEnemySelection(InputAction.CallbackContext ctx)
    {
        action_press.performed -= FinishEnemySelection;
        action_release.performed -= CancelEnemySelection;

        enemy = currentSelection.GetComponent<Enemy>();
        StartPartSelection(enemy);
    }

    private void FinishEnemySelectionVariant()
    {
        enemy = currentSelection.GetComponent<Enemy>();
        StartPartSelection(enemy);

        action_press.performed -= FinishEnemySelection;
        action_release.performed -= CancelEnemySelection;
    }

    public void CancelEnemySelection(InputAction.CallbackContext ctx)
    {
        action_press.performed -= FinishEnemySelection;
        action_release.performed -= CancelEnemySelection;

        currentSelection = null;
        enemy = null;
        part = null;

        menuNav.DisplayMainLayer();
        GetComponent<SpriteRenderer>().color = Color.clear;
        activeEnemies.Clear();
        selectionStage = SelectionStage.None;
    }





    public void StartPartSelection(Enemy enemySelect)
    {
        selectionStage = SelectionStage.Part;

        enemy = enemySelect;
        currentSelection = enemy.transform.GetChild(0).gameObject;
        transform.position = currentSelection.transform.position;

        action.performed += OnNavigate;

        action_press.performed += FinishPartSelection;
        action_release.performed += CancelPartSelection;
    }

    public void FinishPartSelection(InputAction.CallbackContext ctx)
    {
        action.performed -= OnNavigate;
        action_press.performed -= FinishPartSelection;
        action_release.performed -= CancelPartSelection;

        part = currentSelection.GetComponent<EnemyPart>();
        EnterToActionQueue();

        GetComponent<SpriteRenderer>().color = Color.clear;
        activeEnemies.Clear();
        selectionStage = SelectionStage.None;
        currentSelection = null;
        enemy = null;
        part = null;
    }

    public void CancelPartSelection(InputAction.CallbackContext ctx)
    {
        action.performed -= OnNavigate;
        action_press.performed -= FinishPartSelection;
        action_release.performed -= CancelPartSelection;

        currentSelection = null;
        enemy = null;
        part = null;

        StartEnemySelection();
    }



    public void EnterToActionQueue()
    {
        battleManager.EnterToActionQueue(skillIndex, actionType, part);
        menuNav.DisplayMainLayer();
        turnManager.FinishTurn();
    }


    public void MoveSelection(Vector2 inputDir)
    {
        Transform bestCandidate = null;
        Transform fallbackCandidate = null;

        float bestScore = 999f;
        float furthestOpposite = -999f;

        Vector2 currentPos = currentSelection.transform.position;


        if (selectionStage == SelectionStage.Enemy)
        {
            foreach (Enemy candidate in activeEnemies)
            {
                if (candidate.gameObject == currentSelection)
                    continue;

                Vector2 toCandidate = (Vector2)candidate.transform.position - currentPos;
                float dot = Vector2.Dot(inputDir, toCandidate.normalized);

                float distance = toCandidate.magnitude;

                if (dot > 0.5f)
                {
                    if (distance < bestScore)
                    {
                        bestScore = distance;
                        bestCandidate = candidate.transform;
                    }
                }
                else if (dot < -0.5f)
                {
                    if (distance > furthestOpposite)
                    {
                        furthestOpposite = distance;
                        fallbackCandidate = candidate.transform;
                    }
                }
            } // enemy selection
        }

        else if (selectionStage == SelectionStage.Part)
        {
            foreach (Transform candidate in enemy.transform)
            {
                if (candidate.gameObject == currentSelection)
                    continue;

                Vector2 toCandidate = (Vector2)candidate.position - currentPos;
                float dot = Vector2.Dot(inputDir, toCandidate.normalized);

                float distance = toCandidate.magnitude;

                // ✅ Normal directional selection
                if (dot > 0.5f)
                {
                    if (distance < bestScore)
                    {
                        bestScore = distance;
                        bestCandidate = candidate;
                    }
                }
                // ✅ Opposite direction fallback (wrap-around)
                else if (dot < -0.5f)
                {
                    if (distance > furthestOpposite)
                    {
                        furthestOpposite = distance;
                        fallbackCandidate = candidate;
                    }
                }
            }
        }


        Transform finalTarget = bestCandidate != null ? bestCandidate : fallbackCandidate;

        if (finalTarget != null)
        {
            currentSelection = finalTarget.gameObject;
            transform.position = currentSelection.transform.position;
        }
    }

    private void OnNavigate(InputAction.CallbackContext ctx) 
    { 
        Vector2 input = ctx.ReadValue<Vector2>(); 
        if (Mathf.Abs(input.x) > 0.5f) MoveSelection(input.x > 0 ? Vector2.right : Vector2.left); 
    } 

    /*public void MoveSelection(int direction)
    {
        if (activeEnemies.Count == 0)
            return;

        currentSelectionIndex =
            (currentSelectionIndex + direction + activeEnemies.Count)
            % activeEnemies.Count;

        currentSelection = activeEnemies[currentSelectionIndex].gameObject;
        transform.position = currentSelection.transform.position;
    }

    private void OnNavigate(InputAction.CallbackContext ctx)
    {
        Vector2 input = ctx.ReadValue<Vector2>();
        if (Mathf.Abs(input.x) > 0.5f) MoveSelection(input.x > 0 ? 1 : -1);
    } */
}
