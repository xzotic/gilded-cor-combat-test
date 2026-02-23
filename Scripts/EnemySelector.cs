using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class EnemySelector1 : MonoBehaviour
{
    private enum SelectionState
    {
        None,
        Enemy,
        Part
    }

    [Header("References")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private EnemySelectorGeneral general;
    [SerializeField] private GameObject enemyHolder;

    private InputActionMap actionMap;
    private InputAction navigateAction;
    private InputAction submitAction;
    private InputAction cancelAction;

    private SelectionState currentState = SelectionState.None;

    private List<Enemy> activeEnemies = new();
    private Enemy currentEnemy;
    private GameObject currentSelection;
    private EnemyPart currentPart;

    private void OnEnable()
    {
        playerInput = GetComponent<PlayerInput>();

        actionMap = playerInput.actions.FindActionMap("Combat Menu");
        navigateAction = actionMap.FindAction("Navigation");
        submitAction = actionMap.FindAction("Submit");
        cancelAction = actionMap.FindAction("Cancel");

        navigateAction.performed += OnNavigate;
        submitAction.performed += OnSubmit;
        cancelAction.performed += OnCancel;

        actionMap.Disable();
    }

    private void OnDisable()
    {
        navigateAction.performed -= OnNavigate;
        submitAction.performed -= OnSubmit;
        cancelAction.performed -= OnCancel;
    }

    // ================================
    // START ENEMY SELECTION
    // ================================
    public void StartEnemySelection()
    {
        general.DisableMain();
        actionMap.Enable();

        activeEnemies.Clear();

        foreach (Transform child in enemyHolder.transform)
        {
            if (!child.gameObject.activeInHierarchy)
                continue;

            Enemy enemy = child.GetComponent<Enemy>();
            if (enemy != null)
                activeEnemies.Add(enemy);
        }

        if (activeEnemies.Count == 0)
            return;

        currentEnemy = activeEnemies[0];
        currentSelection = currentEnemy.gameObject;

        transform.position = currentSelection.transform.position;
        currentState = SelectionState.Enemy;
    }

    // ================================
    // INPUT HANDLERS
    // ================================
    private void OnSubmit(InputAction.CallbackContext ctx)
    {
        if (currentState == SelectionState.Enemy)
        {
            StartPartSelection(currentSelection.GetComponent<Enemy>());
        }
        else if (currentState == SelectionState.Part)
        {
            general.selectedEnemy = currentEnemy;
            general.selectedEnemyPart = currentSelection.GetComponent<EnemyPart>();
            general.EnterToActionQueue();
            ExitSelection();
        }
    }

    private void OnCancel(InputAction.CallbackContext ctx)
    {
        if (currentState == SelectionState.Part)
        {
            currentState = SelectionState.Enemy;
            currentSelection = currentEnemy.gameObject;
            transform.position = currentSelection.transform.position;
        }
        else if (currentState == SelectionState.Enemy)
        {
            ExitSelection();
            general.DisplayMain();
        }
    }

    private void OnNavigate(InputAction.CallbackContext ctx)
    {
        Vector2 input = ctx.ReadValue<Vector2>();

        if (input.sqrMagnitude < 0.5f)
            return;

        Vector2 dir = Vector2.zero;

        if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
            dir = input.x > 0 ? Vector2.right : Vector2.left;
        else
            dir = input.y > 0 ? Vector2.up : Vector2.down;

        if (currentState == SelectionState.Enemy)
            MoveEnemySelection(dir);
        else if (currentState == SelectionState.Part)
            MovePartSelection(dir);
    }

    // ================================
    // ENEMY SELECTION
    // ================================
    private void MoveEnemySelection(Vector2 inputDir)
    {
        Transform best = GetBestCandidate(
            currentSelection.transform,
            activeEnemies.ConvertAll(e => e.transform),
            inputDir
        );

        if (best != null)
        {
            currentSelection = best.gameObject;
            currentEnemy = best.GetComponent<Enemy>();
            transform.position = best.position;
        }
    }

    // ================================
    // PART SELECTION
    // ================================
    private void StartPartSelection(Enemy enemy)
    {
        currentEnemy = enemy;
        currentSelection = enemy.transform.GetChild(0).gameObject;
        transform.position = currentSelection.transform.position;
        currentState = SelectionState.Part;
    }

    private void MovePartSelection(Vector2 inputDir)
    {
        List<Transform> parts = new();

        foreach (Transform child in currentEnemy.transform)
        {
            if (child.gameObject.activeInHierarchy)
                parts.Add(child);
        }

        Transform best = GetBestCandidate(
            currentSelection.transform,
            parts,
            inputDir
        );

        if (best != null)
        {
            currentSelection = best.gameObject;
            transform.position = best.position;
        }
    }

    // ================================
    // SHARED DIRECTIONAL LOGIC
    // ================================
    private Transform GetBestCandidate(
        Transform current,
        List<Transform> candidates,
        Vector2 inputDir)
    {
        Transform bestCandidate = null;
        Transform fallback = null;

        float bestScore = float.MaxValue;
        float furthestOpposite = float.MinValue;

        Vector2 currentPos = current.position;

        foreach (Transform candidate in candidates)
        {
            if (candidate == current)
                continue;

            Vector2 to = (Vector2)candidate.position - currentPos;
            float dot = Vector2.Dot(inputDir, to.normalized);
            float dist = to.magnitude;

            if (dot > 0.5f)
            {
                if (dist < bestScore)
                {
                    bestScore = dist;
                    bestCandidate = candidate;
                }
            }
            else if (dot < -0.5f)
            {
                if (dist > furthestOpposite)
                {
                    furthestOpposite = dist;
                    fallback = candidate;
                }
            }
        }

        return bestCandidate != null ? bestCandidate : fallback;
    }

    private void ExitSelection()
    {
        actionMap.Disable();
        currentState = SelectionState.None;
        activeEnemies.Clear();
        currentEnemy = null;
        currentSelection = null;
        currentPart = null;
    }
}