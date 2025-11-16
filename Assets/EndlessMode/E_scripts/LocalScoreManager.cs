using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;

public class LocalScoreManager : MonoBehaviour
{
    public static LocalScoreManager Instance;

    private List<int> scores = new List<int>();           // 점수 리스트
    private List<string> names = new List<string>();      // 이름 리스트
    private List<string> ids = new List<string>();        // 고유 ID 리스트

    private string[] rankingData;                         // 파일 저장용 라인 버퍼
    [SerializeField] public string DBFilePath;

    private const int MAX_RANK = 10;

    // 현재 플레이어 정보
    public TextMeshProUGUI currentRankText;
    private string currentPlayerID = "";
    public string currentPlayerName = "";
    private int currentPlayerScore = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        DBFilePath = Path.Combine(Application.persistentDataPath, "rankingDB.txt");
        rankingData = new string[MAX_RANK];
    }

    private void Start()
    {
        // 1. 기존 랭킹 로드
        LoadScoresFromFile();

        // 2. 현재 플레이어 정보 (필요한 방식으로 가져올 것)
        currentPlayerName = GetCurrentPlayerNameSafely();
        currentPlayerScore = ScoreManager.instance.score;
        currentPlayerID = Guid.NewGuid().ToString(); // ← 고유 ID 생성

        // 3. 추가 + 정렬 + 저장 + 내 순위 찾기까지
        AddNewScore(currentPlayerScore, currentPlayerName, currentPlayerID);
    }

    private string GetCurrentPlayerNameSafely()
    {
        if (currentPlayerName != null)
        {
            return currentPlayerName;
        }
        return "Player";
    }

    // 기존 함수 유지
    public void AddNewScore(int newScore)
    {
        AddNewScore(newScore, "Player", Guid.NewGuid().ToString());
    }

    // 실제 처리
    public void AddNewScore(int newScore, string playerName, string id)
    {
        names.Add(playerName);
        scores.Add(newScore);
        ids.Add(id);

        SortByScoreDesc();
        UpdateRankingData();
        SaveScores();

        ShowCurrentPlayerRank(id, playerName, newScore);
    }

    private void LoadScoresFromFile()
    {
        if (!File.Exists(DBFilePath)) return;

        try
        {
            string loaded = File.ReadAllText(DBFilePath);
            string[] lines = loaded.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            names.Clear();
            scores.Clear();
            ids.Clear();

            foreach (var line in lines)
            {
                // rank,name,score,id
                var parts = line.Split(',');
                if (parts.Length < 4) continue;

                string name = parts[1].Trim();
                int score;
                if (!int.TryParse(parts[2].Trim(), out score)) continue;
                string id = parts[3].Trim();

                names.Add(name);
                scores.Add(score);
                ids.Add(id);
            }

            SortByScoreDesc();
        }
        catch (Exception e)
        {
            Debug.LogWarning("랭킹 로드 오류: " + e.Message);
        }
    }

    private void SortByScoreDesc()
    {
        // 버블 정렬 (데이터량 적어서 충분)
        for (int i = 0; i < scores.Count - 1; i++)
        {
            for (int j = i + 1; j < scores.Count; j++)
            {
                if (scores[j] > scores[i])
                {
                    (scores[i], scores[j]) = (scores[j], scores[i]);
                    (names[i], names[j]) = (names[j], names[i]);
                    (ids[i], ids[j]) = (ids[j], ids[i]);
                }
            }
        }
    }

    private void UpdateRankingData()
    {
        int count = Mathf.Min(MAX_RANK, scores.Count);

        // 저장 포맷: rank,name,score,id
        for (int i = 0; i < count; i++)
            rankingData[i] = $"{i + 1},{names[i]},{scores[i]},{ids[i]}";

        // 부족한 부분 "-"
        for (int i = count; i < MAX_RANK; i++)
            rankingData[i] = $"{i + 1},-,-,-";
    }

    private void SaveScores()
    {
        string allData = string.Join("\n", rankingData);
        File.WriteAllText(DBFilePath, allData);
        Debug.Log("랭킹 저장 완료 : " + DBFilePath);
    }

    private void ShowCurrentPlayerRank(string id, string name, int score)
    {
        int rankIndex = ids.IndexOf(id);

        if (rankIndex != -1)
        {
            int rank = rankIndex + 1;

            if (currentRankText != null)
                currentRankText.text = $"내 순위: {rank}위\n이름: {name}\n점수: {score}";

            Debug.Log($"내 순위: {rank}위 / 이름: {name} / 점수: {score}");
        }
    }
}
