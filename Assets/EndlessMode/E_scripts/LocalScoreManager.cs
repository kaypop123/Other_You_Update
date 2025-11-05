using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LocalScoreManager : MonoBehaviour
{
    public static LocalScoreManager Instance;

    private List<int> scores = new List<int>();        // 기존 변수명 유지
    private List<string> names = new List<string>();   // 점수와 1:1 매칭되는 이름 리스트
    private string[] rankingData;                      // 저장/출력용 라인 버퍼 (rank,name,score)
    [SerializeField] public string DBFilePath;
    [SerializeField] public string playerName;

    // 환경에 따라 원하는 길이로 조절 가능 (UI 개수와 맞추면 편함)
    private const int MAX_RANK = 200;

    private void Awake()
    {
        // 싱글톤 베이직
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        // 경로 세팅
        DBFilePath = Path.Combine(Application.persistentDataPath, "rankingDB.txt");
        rankingData = new string[MAX_RANK];
    }

    private void Start()
    {
        // 1) 기존 파일 로드해서 scores/names 채우기
        LoadScoresFromFile();

        // 2) 현재 플레이 결과 추가 (이름은 호출부에서 넘겨주는 게 가장 안전)
        //    -> 예시: AddNewScore(ScoreManager.instance.score, CurrentPlayerName);
        //    기존 시그니처를 최대한 유지하려면 오버로드 제공
        AddNewScore(ScoreManager.instance.score, GetCurrentPlayerNameSafely());
    }

    // 현재 플레이어 이름을 가져오는 부분은 프로젝트 사정에 맞게 교체하세요.
    private string GetCurrentPlayerNameSafely()
    {
        // 예시: PlayerNameManager.Instance.PlayerName가 있다면 그걸 반환
        if (playerName != null)
        {
            return playerName;
        }
        // 없으면 "Player" 같은 기본값
        return "Player";
    }

    // === 기존 시그니처 유지용 (이름 미제공 시 기본 이름으로 처리) ===
    public void AddNewScore(int newScore)
    {
        AddNewScore(newScore, "Player");
    }

    // === 실제 사용 권장: 점수 + 이름을 함께 추가 ===
    public void AddNewScore(int newScore, string playerName)
    {
        // 새 정보 추가
        names.Add(playerName);
        scores.Add(newScore);

        // 내림차순 정렬 (이름/점수 페어를 함께 정렬)
        SortByScoreDesc();

        // 파일로 저장할 랭킹 라인 재구성
        UpdateRankUI();

        // 파일로 저장
        SaveScores();
    }

    // 파일을 읽어 기존 랭킹을 scores/names로 복구
    private void LoadScoresFromFile()
    {
        if (!File.Exists(DBFilePath)) return;

        try
        {
            string loaded = File.ReadAllText(DBFilePath);
            string[] lines = loaded.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            names.Clear();
            scores.Clear();

            for (int i = 0; i < lines.Length; i++)
            {
                // 기대 포맷: rank,name,score
                // rank는 재계산하므로 파싱 실패해도 무시 가능
                var parts = lines[i].Split(',');
                if (parts.Length < 3) continue;

                string namePart = parts[1].Trim();
                string scorePart = parts[2].Trim();

                int parsedScore;
                if (!int.TryParse(scorePart, out parsedScore)) continue;

                names.Add(namePart);
                scores.Add(parsedScore);
            }

            // 혹시 파일이 무질서하게 저장되어 있을 수도 있으니 정렬 한 번
            SortByScoreDesc();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"랭킹 파일 로드 중 오류: {e.Message}");
        }
    }

    // (이름,점수) 페어를 점수 내림차순으로 정렬
    private void SortByScoreDesc()
    {
        // 간단하게 버블/선택정렬도 되지만, 이해 쉬운 1패스 정렬
        // 리스트 크기가 작으니 성능 문제 없음
        for (int i = 0; i < scores.Count - 1; i++)
        {
            for (int j = i + 1; j < scores.Count; j++)
            {
                if (scores[j] > scores[i])
                {
                    // 점수 스왑
                    int ts = scores[i];
                    scores[i] = scores[j];
                    scores[j] = ts;

                    // 이름 스왑
                    string tn = names[i];
                    names[i] = names[j];
                    names[j] = tn;
                }
            }
        }
    }

    // rankingData(문자열 라인들) 구성: "rank,name,score"
    private void UpdateRankUI()
    {
        // 상위 MAX_RANK까지만 저장
        int count = Mathf.Min(MAX_RANK, scores.Count);

        for (int i = 0; i < count; i++)
        {
            int rank = i + 1;
            string name = names[i];
            int score = scores[i];

            rankingData[i] = $"{rank},{name},{score}";
        }

        // 부족하면 비워주거나 기본값으로 채우기
        for (int i = count; i < MAX_RANK; i++)
        {
            // 포맷 유지: rank,name,score
            rankingData[i] = $"{i + 1},-,-";
        }
    }

    private void SaveScores()
    {
        try
        {
            string allData = string.Join("\n", rankingData);
            File.WriteAllText(DBFilePath, allData);   // 없으면 생성, 있으면 덮어쓰기
            Debug.Log($"랭킹 저장 완료 → {DBFilePath}");
        }
        catch (Exception e)
        {
            Debug.LogWarning($"랭킹 저장 실패: {e.Message}");
        }
    }
}