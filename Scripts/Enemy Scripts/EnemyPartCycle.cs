using UnityEngine;
using UnityEngine.InputSystem;

public class EnemyPartCycle : MonoBehaviour
{
    private PlayerInput playerInput;
    private InputActionMap actionMap;
    private InputAction navigateAction;
    private InputAction submitAction;
    private InputAction cancelAction;

    [SerializeField] private Enemy enemy;
    [SerializeField] private BattleManager battleManager;

    public GameObject currentSelection;

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

    public void StartSelection(Enemy selectedEnemy)
    {
        enemy = selectedEnemy;

        if (enemy == null)
        {
            battleManager.partSelectStopFlag = true;
            return;
        }

        if (enemy.transform.childCount > 0)
        {
            currentSelection = enemy.transform.GetChild(0).gameObject;
            transform.position = currentSelection.transform.position;
        }

        battleManager.partSelectStopFlag = false;

        navigateAction.performed += OnNavigate;
        submitAction.performed += StopSelection;
        cancelAction.performed += CancelSelection;
    }

    public void StopSelection(InputAction.CallbackContext ctx)
    {
        enemy.currentSelectedPart = currentSelection;
        FinishSelection();
    }

    public void CancelSelection(InputAction.CallbackContext ctx)
    {
        enemy.currentSelectedPart = null;
        FinishSelection();
    }

    private void FinishSelection()
    {
        UnbindActions();
        battleManager.partSelectStopFlag = true;
    }

    private void UnbindActions()
    {
        if (navigateAction != null) navigateAction.performed -= OnNavigate;
        if (submitAction != null) submitAction.performed -= StopSelection;
        if (cancelAction != null) cancelAction.performed -= CancelSelection;
    }

    public void MoveSelection(Vector2 inputDir)
    {
        if (enemy == null || currentSelection == null)
        {
            return;
        }

        Transform bestCandidate = null;
        Transform fallbackCandidate = null;

        float bestScore = float.MaxValue;
        float furthestOpposite = float.MinValue;

        Vector2 currentPos = currentSelection.transform.position;

        foreach (Transform candidate in enemy.transform)
        {
            if (candidate.gameObject == currentSelection)
                continue;

            Vector2 toCandidate = (Vector2)candidate.position - currentPos;
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
