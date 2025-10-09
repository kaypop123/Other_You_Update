using UnityEngine;

public class GameOverRankUpdater : MonoBehaviour
{
    private RankUI rankUI;
    private ScoreManager scoreManager;

    void OnEnable()
    {
        // 씬 로드 직후 프레임 한 번 기다리기
        StartCoroutine(InitializeAfterFrame());
    }

    private System.Collections.IEnumerator InitializeAfterFrame()
    {
        yield return null; // 1프레임 대기 (씬 로드 후 RankUI 활성화 대기)

        rankUI = FindObjectOfType<RankUI>();
        scoreManager = FindObjectOfType<ScoreManager>();

        if (rankUI == null || scoreManager == null)
        {
            Debug.LogWarning("[RankUpdater] RankUI 또는 ScoreManager를 찾지 못했습니다!");
            yield break;
        }

        rankUI.AddNewScore(scoreManager.score);
        Debug.Log("[RankUpdater] 점수 갱신 완료!");
    }
}
