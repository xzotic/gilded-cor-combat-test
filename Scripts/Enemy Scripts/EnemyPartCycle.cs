using UnityEngine;
using UnityEngine.InputSystem;

public class EnemyPartCycle : MonoBehaviour
{
    private PlayerInput playerInput;
    private InputActionMap actionMap;
    private InputAction action;
    private InputAction action_press;
    private InputAction action_release;
    private Enemy enemy;
    [SerializeField] private EnemySelector enemySelector;
    [SerializeField] private EnemySelectorGeneral general;

    public GameObject currentSelection;

    private void OnEnable()
    {
        playerInput = GetComponent<PlayerInput>();
        actionMap = playerInput.actions.FindActionMap("Combat Menu");
        action = actionMap.FindAction("Navigation");
        action_press = actionMap.FindAction("Submit");
        action_release = actionMap.FindAction("Cancel");
    }

    private void OnDisable()
    {
        action.performed -= OnNavigate;
        action_press.performed -= FinishPartSelection;
        action_release.performed -= CancelPartSelection;
    }

    public void StartPartSelection(Enemy enemySelect)
    {
        enemy = enemySelect;
        currentSelection = enemy.transform.GetChild(0).gameObject;
        transform.position = currentSelection.transform.position;

        action.performed += OnNavigate;

        action_press.performed += FinishPartSelection;
        action_release.performed += CancelPartSelection;
    }

    public void FinishPartSelection(InputAction.CallbackContext ctx)
    {
        enemy = null;
        general.selectedEnemyPart = currentSelection.GetComponent<EnemyPart>();
        general.EnterToActionQueue();

        action.performed -= OnNavigate;
        action_press.performed -= FinishPartSelection;
        action_release.performed -= CancelPartSelection;
    }

    public void CancelPartSelection(InputAction.CallbackContext ctx)
    {
        enemy = null;
        enemySelector.StartEnemySelection();

        action.performed -= OnNavigate;
        action_press.performed -= FinishPartSelection;
        action_release.performed -= CancelPartSelection;
    }


    public void MoveSelection(Vector2 inputDir)
    {
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