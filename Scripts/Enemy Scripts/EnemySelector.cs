using UnityEngine;
using UnityEngine.InputSystem;

public class EnemySelector : MonoBehaviour
{
    private PlayerInput playerInput;
    private InputActionMap actionMap;
    private InputAction navigateAction;
    private InputAction submitAction;
    private InputAction cancelAction;

    private Enemy[] activeEnemies;
    private BattleManager battleManager;

    public Enemy selectedEnemy;

    private void OnEnable()
    {
        playerInput = GetComponent<PlayerInput>();
        actionMap = playerInput.actions.FindActionMap("Combat Menu");
        navigateAction = actionMap.FindAction("Navigation");
        submitAction = actionMap.FindAction("Submit");
        cancelAction = actionMap.FindAction("Cancel");
    }

    private void OnDisable()
    {
        UnbindActions();
    }

    public void StartSelection(Enemy[] enemies, BattleManager manager)
    {
        activeEnemies = enemies;
        battleManager = manager;

        if (activeEnemies == null || activeEnemies.Length == 0)
        {
            selectedEnemy = null;
            battleManager.enemySelectStopFlag = true;
            return;
        }

        if (selectedEnemy == null)
        {
            selectedEnemy = activeEnemies[0];
        }

        transform.position = selectedEnemy.transform.position;

        battleManager.enemySelectStopFlag = false;

        navigateAction.performed += OnNavigate;
        submitAction.performed += ConfirmSelection;
        cancelAction.performed += CancelSelection;
    }

    private void ConfirmSelection(InputAction.CallbackContext ctx)
    {
        FinishSelection(selectedEnemy);
    }

    private void CancelSelection(InputAction.CallbackContext ctx)
    {
        FinishSelection(null);
    }

    private void FinishSelection(Enemy enemy)
    {
        selectedEnemy = enemy;
        UnbindActions();
        battleManager.enemySelectStopFlag = true;
    }

    private void UnbindActions()
    {
        if (navigateAction != null) navigateAction.performed -= OnNavigate;
        if (submitAction != null) submitAction.performed -= ConfirmSelection;
        if (cancelAction != null) cancelAction.performed -= CancelSelection;
    }

    private void MoveSelection(Vector2 inputDir)
    {
        if (selectedEnemy == null)
        {
            return;
        }

        Enemy bestCandidate = null;
        Enemy fallbackCandidate = null;

        float bestScore = float.MaxValue;
        float furthestOpposite = float.MinValue;

        Vector2 currentPos = selectedEnemy.transform.position;

        foreach (Enemy candidate in activeEnemies)
        {
            if (candidate == selectedEnemy)
            {
                continue;
            }

            Vector2 toCandidate = (Vector2)candidate.transform.position - currentPos;
            float dot = Vector2.Dot(inputDir, toCandidate.normalized);
            float distance = toCandidate.magnitude;

            if (dot > 0.5f)
            {
                if (distance < bestScore)
                {
                    bestScore = distance;
                    bestCandidate = candidate;
                }
            }
            else if (dot < -0.5f)
            {
                if (distance > furthestOpposite)
                {
                    furthestOpposite = distance;
                    fallbackCandidate = candidate;
                }
            }
        }

        Enemy finalTarget = bestCandidate != null ? bestCandidate : fallbackCandidate;

        if (finalTarget != null)
        {
            selectedEnemy = finalTarget;
            transform.position = selectedEnemy.transform.position;
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
