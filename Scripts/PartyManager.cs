// ================================
// PartyManager.cs (FULL, FINAL)
// ================================

using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PartyManager : MonoBehaviour
{
    [Header("Party")]
    public int[] activeMembers;
    private int activeCount;
    private int cycle;

    [Header("Portrait UI")]
    [SerializeField] private Image[] portraitHolder;
    [SerializeField] private Sprite[] portraits;

    [Header("Stats UI")]
    [SerializeField] private TextMeshProUGUI hpHolder;
    [SerializeField] private TextMeshProUGUI spHolder;

    [Header("Portrait Slots")]
    [SerializeField] private float moveSpeed = 10f;

    private Vector2[] pos = new Vector2[4];
    private float[] scale = new float[4];
    private float[] rot = new float[4];
    private Color[] colors = new Color[4];

    private Coroutine portraitMoveRoutine;

    private void Awake()
    {
        activeCount = Mathf.Clamp(activeMembers.Length, 1, 4);
        cycle = 0;

        // Positions
        pos[0] = new Vector2(-54.5f, 25.4f);
        pos[1] = new Vector2(63f, -13.6f);
        pos[2] = new Vector2(152f, -55.4f);
        pos[3] = new Vector2(-146f, -51f);

        // Scales
        scale[0] = 1f;
        scale[1] = scale[2] = scale[3] = 0.7f;

        // Rotations (Z)
        rot[0] = 0f;
        rot[1] = -7f;
        rot[2] = -15f;
        rot[3] = 10f;

        // Colors (front → back)
        colors[0] = new Color32(255, 255, 255, 255);
        colors[1] = new Color32(180, 180, 180, 255);
        colors[2] = new Color32(120, 120, 120, 255);
        colors[3] = new Color32(60, 60, 60, 255);

        EnablePortraits();
        AssignPortraitSprites();
        SnapPortraitsToSlots();
        UpdatePortraitRenderOrder();
    }

    /* ===============================
       PORTRAIT SYSTEM
       =============================== */

    void EnablePortraits()
    {
        for (int i = 0; i < portraitHolder.Length; i++)
            portraitHolder[i].gameObject.SetActive(i < activeCount);
    }

    void AssignPortraitSprites()
    {
        for (int i = 0; i < activeCount; i++)
            portraitHolder[i].sprite = portraits[activeMembers[i]];
    }

    IEnumerator AnimatePortraits()
    {
        while (true)
        {
            bool done = true;

            for (int i = 0; i < activeCount; i++)
            {
                int slotIndex = (i - cycle + activeCount) % activeCount;
                RectTransform t = portraitHolder[i].rectTransform;

                // Position
                t.localPosition = Vector2.Lerp(
                    t.localPosition,
                    pos[slotIndex],
                    Time.deltaTime * moveSpeed
                );

                // Scale
                t.localScale = Vector3.Lerp(
                    t.localScale,
                    Vector3.one * scale[slotIndex],
                    Time.deltaTime * moveSpeed
                );

                // Rotation
                float currentZ = t.localEulerAngles.z;
                float targetZ = rot[slotIndex];
                float newZ = Mathf.LerpAngle(
                    currentZ,
                    targetZ,
                    Time.deltaTime * moveSpeed
                );
                t.localRotation = Quaternion.Euler(0f, 0f, newZ);

                // Color
                portraitHolder[i].color = Color.Lerp(
                    portraitHolder[i].color,
                    colors[slotIndex],
                    Time.deltaTime * moveSpeed
                );

                if (Vector2.Distance(t.localPosition, pos[slotIndex]) > 0.1f ||
                    Mathf.Abs(Mathf.DeltaAngle(currentZ, targetZ)) > 0.5f ||
                    Vector4.Distance(portraitHolder[i].color, colors[slotIndex]) > 0.02f)
                {
                    done = false;
                }
            }

            if (done)
                break;

            yield return null;
        }

        SnapPortraitsToSlots();
    }

    void StartPortraitAnimation()
    {
        if (portraitMoveRoutine != null)
            StopCoroutine(portraitMoveRoutine);

        UpdatePortraitRenderOrder();
        portraitMoveRoutine = StartCoroutine(AnimatePortraits());
    }

    void SnapPortraitsToSlots()
    {
        for (int i = 0; i < activeCount; i++)
        {
            int slotIndex = (i - cycle + activeCount) % activeCount;
            Transform t = portraitHolder[i].transform;

            t.localPosition = pos[slotIndex];
            t.localScale = Vector3.one * scale[slotIndex];
            t.localRotation = Quaternion.Euler(0f, 0f, rot[slotIndex]);
            portraitHolder[i].color = colors[slotIndex];
        }
    }

    void UpdatePortraitRenderOrder()
    {
        for (int slot = activeCount - 1; slot >= 0; slot--)
        {
            int portraitIndex = (slot + cycle) % activeCount;
            portraitHolder[portraitIndex]
                .transform.SetSiblingIndex(activeCount - 1 - slot);
        }
    }

    public void NextTurn()
    {
        cycle = (cycle + 1) % activeCount;
        StartPortraitAnimation();
    }

    public void AnimateResetToOriginalOrder()
    {
        cycle = 0;
        StartPortraitAnimation();
    }

    public void ResetCycle()
    {
        cycle = 0;
        SnapPortraitsToSlots();
        UpdatePortraitRenderOrder();
    }

    /* ===============================
       BATTLE TURN CONTROL
       =============================== */

    public void EnableCharacterTurn(int index)
    {
        transform.GetChild(index).gameObject.SetActive(true);
    }

    public void EndCharacterTurn(int index)
    {
        transform.GetChild(index).gameObject.SetActive(false);
    }

    public void UpdateHPSP(int index)
    {
        BattleCharacter bc = transform.GetChild(index).GetComponent<BattleCharacter>();
        hpHolder.text = $"{bc.effHP} / {bc.baseHP} HP";
        spHolder.text = MentalState(bc.effSP);
    }

    public BattleCharacter GetBattleCharacter(int index)
    {
        return transform.GetChild(index).GetComponent<BattleCharacter>();
    }

    /* ===============================
       MENTAL STATE
       =============================== */

    public string MentalState(int sp)
    {
        if (sp > 90) return "RESOLUTE";
        if (sp > 80) return "UNYIELDING";
        if (sp > 70) return "CONFIDENT";
        if (sp > 60) return "COMPOSED";
        if (sp > 50) return "FOCUSED";
        if (sp > 40) return "HESITANT";
        if (sp > 30) return "WEARY";
        if (sp > 20) return "DESPERATE";
        if (sp > 10) return "BROKEN";
        return "HYSTERICAL";
    }
}
