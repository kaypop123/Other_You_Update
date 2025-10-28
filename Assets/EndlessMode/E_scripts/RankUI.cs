using System;
using System.IO;
using TMPro;
using UnityEngine;

public class RankUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI[] rankText;

    void Start()
    {
        LoadRank();
    }

    private void LoadRank()
    {
        // LocalScoreManager가 씬에 존재한다는 전제
        if (LocalScoreManager.Instance == null)
        {
            Debug.LogWarning("LocalScoreManager.Instance가 없습니다.");
            return;
        }

        string path = LocalScoreManager.Instance.DBFilePath;

        if (File.Exists(path))
        {
            try
            {
                string loaded = File.ReadAllText(path);
                string[] lines = loaded.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

                for (int i = 0; i < rankText.Length; i++)
                {
                    if (i < lines.Length)
                    {
                        
                        var parts = lines[i].Split(',');
                        if (parts.Length >= 3)
                        {
                            string rank = parts[0].Trim();
                            string name = parts[1].Trim();
                            string score = parts[2].Trim();
                            rankText[i].text = $"{rank}위 / name: {name} / score: {score}";
                        }
                        else
                        {
                            rankText[i].text = lines[i];
                        }
                    }
                    else
                    {
                        // 파일 라인보다 UI가 더 많을 때는 비워두기
                        rankText[i].text = "";
                    }
                }

                Debug.Log("랭킹 불러오기 완료!");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"랭킹 로드 실패: {e.Message}");
            }
        }
        else
        {
            Debug.LogWarning("저장된 메모 파일이 없습니다!");
            // 파일이 없으면 UI를 기본값으로 정리
            for (int i = 0; i < rankText.Length; i++)
                rankText[i].text = "";
        }
    }
}
