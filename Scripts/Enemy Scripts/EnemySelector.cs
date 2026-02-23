using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class EnemySelector : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    private InputActionMap actionMap;
    private InputAction action;
    private InputAction action_press;
    private InputAction action_release;
    public bool selectedFlag = false;
    [SerializeField] private EnemySelectorGeneral general;
    [SerializeField] private EnemyPartCycle partCycle;

    public GameObject enemyHolder;
    public List<Enemy> activeEnemies = new List<Enemy>();
    public GameObject currentSelection;


    private void OnEnable()
    {
        GetComponent<SpriteRenderer>().color = Color.clear;
        playerInput = GetComponent<PlayerInput>();
        actionMap = playerInput.actions.FindActionMap("Combat Menu");
        action = actionMap.FindAction("Navigation");
        action_press = actionMap.FindAction("Submit");
        action_release = actionMap.FindAction("Cancel");

        actionMap.Disable();
    }

    public void StartEnemySelection()
    {
        general.DisableMain();
        actionMap.Enable();

        GetComponent<SpriteRenderer>().color = Color.red;
        currentSelection = GameObject.FindWithTag("Enemy");
        transform.position = currentSelection.transform.position;


        action.performed += OnNavigate;
        selectedFlag = false;

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

        action_press.performed += FinishEnemySelection;
        action_release.performed += CancelEnemySelection;
    }

    public void FinishEnemySelection(InputAction.CallbackContext ctx)
    {
        selectedFlag = true;
        general.selectedEnemy = currentSelection.GetComponent<Enemy>();
        partCycle.StartPartSelection(general.selectedEnemy);
        
        action_press.performed -= FinishEnemySelection;
        action_release.performed -= CancelEnemySelection;
    }

    private void FinishEnemySelectionVariant()
    {
        selectedFlag = true;
        general.selectedEnemy = currentSelection.GetComponent<Enemy>();
        partCycle.StartPartSelection(general.selectedEnemy);

        action_press.performed -= FinishEnemySelection;
        action_release.performed -= CancelEnemySelection;
    }

    public void CancelEnemySelection(InputAction.CallbackContext ctx)
    {
        selectedFlag = false;
        general.selectedEnemy = null;
        general.DisplayMain();
        GetComponent<SpriteRenderer>().color = Color.clear;
        activeEnemies.Clear();

        action_press.performed -= FinishEnemySelection;
        action_release.performed -= CancelEnemySelection;
    }

    public void MoveSelection(Vector2 inputDir)
    {
        Transform bestCandidate = null;
        Transform fallbackCandidate = null;

        float bestScore = float.MaxValue;
        float furthestOpposite = float.MinValue;

        Vector2 currentPos = currentSelection.transform.position;

        foreach (Enemy candidate in activeEnemies)
        {
            if (candidate.gameObject == currentSelection)
                continue;

            Vector2 toCandidate = (Vector2)candidate.transform.position - currentPos;
            float dot = Vector2.Dot(inputDir, toCandidate.normalized);

            float distance = toCandidate.magnitude;

            // ✅ Normal directional selection
            if (dot > 0.5f)
            {
                if (distance < bestScore)
                {
                    bestScore = distance;
                    bestCandidate = candidate.transform;
                }
            }
            // ✅ Opposite direction fallback (wrap-around)
            else if (dot < -0.5f)
            {
                if (distance > furthestOpposite)
                {
                    furthestOpposite = distance;
                    fallbackCandidate = candidate.transform;
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

        if (input.y > 0.5f)
            MoveSelection(Vector2.up);
        else if (input.y < -0.5f)
            MoveSelection(Vector2.down);
        else if (input.x > 0.5f)
            MoveSelection(Vector2.right);
        else if (input.x < -0.5f)
            MoveSelection(Vector2.left);
    }
}
