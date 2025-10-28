using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LocalScoreManager : MonoBehaviour
{
    public static LocalScoreManager Instance;

    private List<int> scores = new List<int>();
    [SerializeField] private TextMeshProUGUI[] rankText;

    public void AddNewScore(int newScore)
    {
        scores.Add(newScore);
        scores.Sort((a, b) => b.CompareTo(a)); // 내림차순 정렬

        // 10개까지만 저장
        if (scores.Count > 10)
            scores = scores.GetRange(0, 10);

        UpdateRankUI();
    }

    private void UpdateRankUI()
    {
        for (int i = 0; i < 10; i++)
        {
            rankText[i].text = scores.Count > i ? $"{i + 1}위 : {scores[i]}" : $"{i + 1}위 : -";
        }

        SaveScores();
    }

    private void SaveScores()
    {
        //메모장에 저장
    }
}
