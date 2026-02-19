using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.EventSystems;
//using Unity.AppUI.UI;
using TMPro;
using UnityEngine.UI;

[RequireComponent(typeof(InputSystemUIInputModule))]
public class CombatMenuNav : MonoBehaviour
{
    [Header("Default Selection")]
    [SerializeField] private GameObject defaultButton;

    private InputSystemUIInputModule uiInput;
    private InputActionMap combatMap;
    private InputAction navigate;
    private InputAction pause;

    [SerializeField] private TurnManager turnManager;

    [Header("UI Layers")]
    [SerializeField] private GameObject mainLayer;
    [SerializeField] private GameObject attackLayer;
    [SerializeField] private GameObject attackLayer1;
    [SerializeField] private GameObject defendLayer;
    [SerializeField] private GameObject diceLayer;


    private void Awake()
    {
        uiInput = GetComponent<InputSystemUIInputModule>();

        // Use the SAME actions asset as the UI module
        combatMap = uiInput.actionsAsset.FindActionMap("Combat Menu", true);

        //navigate = combatMap.FindAction("Navigation", true); // Value / Vector2
        pause = combatMap.FindAction("Pause", true);

    }

    private void OnEnable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        combatMap.Enable();

        // Auto-select default button when navigation starts
        //navigate.performed += OnNavigate;
        pause.performed += OnPause;

        DisplayMainLayer();
    }

    private void OnDisable()
    {
        //navigate.performed -= OnNavigate;
        pause.performed -= OnPause;

        combatMap.Disable();
    }



    private void OnNavigate(InputAction.CallbackContext ctx)
    {
        // Ignore tiny noise (especially gamepad sticks)
        if (ctx.ReadValue<Vector2>().sqrMagnitude < 0.01f)
            return;

        if (EventSystem.current.currentSelectedGameObject == null)
        {
            EventSystem.current.SetSelectedGameObject(GameObject.FindWithTag("UI Button"));
        }
    }
    private void OnPause(InputAction.CallbackContext ctx)
    {
        turnManager.gameObject.GetComponent<GlobalData>().PauseGame();
    }


    public void DisplayAttackLayer()
    {
        EventSystem.current.SetSelectedGameObject(null);
        mainLayer.SetActive(false);
        attackLayer.SetActive(true);
        attackLayer1.SetActive(false);
        defendLayer.SetActive(false);


        // Enter Sorcery Skill Turn Action
        for (int i=0;i<6;i++)
        {
            Button tmp = attackLayer.transform.GetChild(i).GetComponent<Button>();
            tmp.interactable = true;
            if (turnManager.ReturnActiveSkill(i) != null)
            {
                tmp.GetComponentInChildren<TextMeshProUGUI>().text = turnManager.ReturnActiveSkill(i).skillName;
                tmp.gameObject.GetComponent<SkillHolder>().skillIndex = i;
                tmp.gameObject.GetComponent<SkillHolder>().actionType = ActionType.SorcerySkill;
            }
            else
            {
                tmp.GetComponentInChildren<TextMeshProUGUI>().text = " ";
                tmp.gameObject.GetComponent<SkillHolder>().skillIndex = -1;
                tmp.interactable = false;
            }
        }
    }

    public void DisplayAttackLayer1()
    {
        EventSystem.current.SetSelectedGameObject(null);
        mainLayer.SetActive(false);
        attackLayer.SetActive(false);
        attackLayer1.SetActive(true);
        defendLayer.SetActive(false);
    }

    public void DisplayDefendLayer()
    {
        EventSystem.current.SetSelectedGameObject(null);
        mainLayer.SetActive(false);
        attackLayer.SetActive(false);
        attackLayer1.SetActive(false);
        defendLayer.SetActive(true);
    } 



    public void DisplayMainLayer()
    {
        EventSystem.current.SetSelectedGameObject(null);
        mainLayer.SetActive(true);
        attackLayer.SetActive(false);
        attackLayer1.SetActive(false);
        defendLayer.SetActive(false);
    }
    public void DisableMainLayer()
    {
        mainLayer.SetActive(false);
    }



    public void DisplayDice(bool a)
    {
        diceLayer.SetActive(a);
    }
}
