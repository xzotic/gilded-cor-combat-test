using UnityEngine;
using UnityEngine.InputSystem;

public class EnemyPartCycle : MonoBehaviour
{
    private PlayerInput playerInput;
    private InputActionMap actionMap;
    private InputAction action;
    private InputAction action_press;
    private InputAction action_release;
    [SerializeField] private Enemy enemy;
    [SerializeField] private BattleManager battleManager;

    public GameObject currentSelection;

    private void OnEnable()
    {
        playerInput = GetComponent<PlayerInput>();
        actionMap = playerInput.actions.FindActionMap("Combat Menu");
        action = actionMap.FindAction("Navigation");
        action_press = actionMap.FindAction("Submit");
    }

    private void OnDisable()
    {
        action.performed -= OnNavigate;
        action_press.performed -= StopSelection;
        action_release.performed -= CancelSelection;
    }

    public void StartSelection()
    {
        enemy = transform.parent.GetComponentInChildren<Enemy>();
        battleManager.partSelectStopFlag = false;
        action.performed += OnNavigate;

        action_press.performed += StopSelection;
        action_release.performed += CancelSelection;
    }

    public void StopSelection(InputAction.CallbackContext ctx)
    {
        enemy = null;
        action.performed -= OnNavigate;
        action_press.performed -= StopSelection;
        action_release.performed -= CancelSelection;

        enemy.currentSelectedPart = currentSelection;
        battleManager.partSelectStopFlag = true;
    }

    public void CancelSelection(InputAction.CallbackContext ctx)
    {
        enemy = null;
        action.performed -= OnNavigate;
        action_press.performed -= StopSelection;
        action_release.performed -= CancelSelection;

        enemy.currentSelectedPart = null;
        battleManager.partSelectStopFlag = true;
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
