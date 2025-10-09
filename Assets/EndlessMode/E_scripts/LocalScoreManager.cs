using UnityEngine;

public class LocalScoreManager : MonoBehaviour
{
    public static LocalScoreManager Instance;

    public int[] topScores = new int[3]; // 상위 3위 점수

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // PlayerPrefs에서 점수 불러오기
        for (int i = 0; i < topScores.Length; i++)
        {
            topScores[i] = PlayerPrefs.GetInt("TopScore" + i, 0);
        }
    }

    public void AddScore(int score)
    {
        for (int i = 0; i < topScores.Length; i++)
        {
            if (score > topScores[i])
            {
                // 점수 밀어내기
                for (int j = topScores.Length - 1; j > i; j--)
                    topScores[j] = topScores[j - 1];

                topScores[i] = score;
                SaveScores();
                break;
            }
        }
    }

    private void SaveScores()
    {
        for (int i = 0; i < topScores.Length; i++)
            PlayerPrefs.SetInt("TopScore" + i, topScores[i]);

        PlayerPrefs.Save();
    }
}
