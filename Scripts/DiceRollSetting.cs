using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DiceRollSetting : MonoBehaviour
{
    private GlobalData globalData;
    private Button rollButton;

    private void Start()
    {
        globalData = GameObject.FindWithTag("Playables").GetComponent<GlobalData>();
        rollButton = GetComponent<Button>();
    }

    void UpdateRollButtonColor()
    {
        Image img = rollButton.GetComponent<Image>();
        TextMeshProUGUI text = rollButton.GetComponentInChildren<TextMeshProUGUI>();

        switch ((int)globalData.rollSetting)
        {
            case 0:
                img.color = Color.white;
                text.text = "Manual";
                break;

            case 1:
                img.color = Color.yellow;
                text.text = "Auto";
                break;

            case 2:
                img.color = Color.green;
                text.text = "Take 10";
                break;
        }
    }

    public void CycleRollSetting() // cycle to next roll setting
    {
        int next = ((int)globalData.rollSetting + 1) %
            System.Enum.GetValues(typeof(GlobalData.DiceRollSetting)).Length;

        globalData.rollSetting = (GlobalData.DiceRollSetting)next;

        UpdateRollButtonColor();
    }
}
