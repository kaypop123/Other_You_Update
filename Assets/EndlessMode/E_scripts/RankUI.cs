using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RankUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI rank1Text;
    [SerializeField] private TextMeshProUGUI rank2Text;
    [SerializeField] private TextMeshProUGUI rank3Text;

    private List<int> scores = new List<int>();

    void Start()
    {
        LoadRank();
        UpdateRankUI();
    }

    public void AddNewScore(int newScore)
    {
        scores.Add(newScore);
        scores.Sort((a, b) => b.CompareTo(a)); // 내림차순 정렬

        // 3개까지만 저장
        if (scores.Count > 3)
            scores = scores.GetRange(0, 3);

        SaveRank();
        UpdateRankUI();
    
    }

    private void UpdateRankUI()
    {
        rank1Text.text = scores.Count > 0 ? $"1위 : {scores[0]}" : "1위 : -";
        rank2Text.text = scores.Count > 1 ? $"2위 : {scores[1]}" : "2위 : -";
        rank3Text.text = scores.Count > 2 ? $"3위 : {scores[2]}" : "3위 : -";
    }

    private void SaveRank()
    {
        for (int i = 0; i < scores.Count; i++)
        {
            PlayerPrefs.SetInt("Rank" + i, scores[i]);
        }
        PlayerPrefs.Save();
    }

    private void LoadRank()
    {
        scores.Clear();
        for (int i = 0; i < 3; i++)
        {
            if (PlayerPrefs.HasKey("Rank" + i))
                scores.Add(PlayerPrefs.GetInt("Rank" + i));
        }
    }
}
