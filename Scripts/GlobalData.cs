using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class GlobalData : MonoBehaviour
{
    public int level;
    public DiceRollSetting rollSetting;
    public bool isPaused = false;
    [SerializeField] private GameObject pauseCatch;
    [SerializeField] private int targetFPS;

    private void Start()
    {
        SetTargetFPS(targetFPS);
    }

    public enum DiceRollSetting
    {
        Manual=0,
        Auto=1,
        TakeTen=2
    }

    public void PauseGame()
    {
        if (isPaused == true)
        {
            isPaused = false;
            GameObject.FindGameObjectWithTag("Pause Button").GetComponentInChildren<TextMeshProUGUI>().text = "No";
            pauseCatch.SetActive(false);
            Time.timeScale = 1;
        } else
        {
            isPaused = true;
            GameObject.FindGameObjectWithTag("Pause Button").GetComponentInChildren<TextMeshProUGUI>().text = "Yes";
            pauseCatch.SetActive(true);
            Time.timeScale = 0;
        }
    }

    public void SetTargetFPS(int target)
    {
        Application.targetFrameRate = target;
    }
}
