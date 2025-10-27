using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance;

    public TextMeshProUGUI stageText; // 스테이지 넘어갈 때마다 띄울 거
    public TextMeshProUGUI stageDisplayText; // 상시로 띄울 스테이지
    public CanvasGroup stageBackground; // 반투명 배경용 CanvasGroup

    static public int currentStage = 1;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        stageText.gameObject.SetActive(false);
        stageBackground.alpha = 0; // 투명 초기화
        UpdateStageDisplay(); // 게임 시작 시 우측 상단에 표시

        StartCoroutine(ShowInitialStage());
    }

    public void IncreaseStageAndShow()
    {
        currentStage++;
        UpdateStageDisplay(); // 스테이지 올라갈 때 즉시 갱신
        StartCoroutine(ShowStageText());
    }

    private IEnumerator ShowInitialStage()
    {
        yield return ShowStageWithBackground(currentStage);
    }

    private void UpdateStageDisplay()
    {
        stageDisplayText.text = $"STAGE {currentStage}";
    }

    private IEnumerator ShowStageText()
    {
        yield return ShowStageWithBackground(currentStage);
    }

    // 공통 함수: 배경 + 텍스트
    private IEnumerator ShowStageWithBackground(int stageNumber)
    {
        stageText.text = $"STAGE {stageNumber}";
        stageText.gameObject.SetActive(true);

        // 배경 Fade In
        stageBackground.DOFade(0.6f, 0.3f); // alpha 0 → 0.6
        stageText.DOFade(1f, 0.3f);

        yield return new WaitForSeconds(0.9f);

        // Fade Out
        stageBackground.DOFade(0, 0.3f);
        stageText.DOFade(0, 0.3f);

        yield return new WaitForSeconds(0.3f);

        stageText.gameObject.SetActive(false);
    }

    public int GetCurrentStage() => currentStage;
}