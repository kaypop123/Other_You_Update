using UnityEngine;

public class GameOverRankHandler : MonoBehaviour
{
    public GameObject rankUI; // RankUI 오브젝트 연결

    private RankUI rankUIScript;

    void Start()
    {
        if (rankUI != null)
            rankUIScript = rankUI.GetComponent<RankUI>();
    }

    // ✅ 게임오버 시 이 함수 한 번 호출하면 끝!
    public void ShowRankUI()
    {
        if (rankUI != null)
        {
            rankUI.SetActive(true);
            rankUIScript.AddNewScore(ScoreManager.instance.score);
        }
        else
        {
            Debug.LogWarning("⚠ RankUI가 연결되어 있지 않습니다!");
        }
    }
}
