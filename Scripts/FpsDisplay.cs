using UnityEngine;
using TMPro;

public class FPSCounter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI fpsText;

    private float timer;

    private void Update()
    {
        timer += Time.unscaledDeltaTime;

        if (timer >= 0.25f) // update 4 times per second
        {
            float fps = 1f / Time.unscaledDeltaTime;
            fpsText.text = Mathf.RoundToInt(fps) + " FPS";
            timer = 0f;
        }
    }
}