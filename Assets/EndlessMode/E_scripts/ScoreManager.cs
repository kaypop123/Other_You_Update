using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;
    public int score = 0;

    public TextMeshProUGUI scoreLabel;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    public void AddScore(int value)
    {
        score += value;
        scoreLabel.text = ((int)score).ToString();
        Debug.Log("Score: " + score); // 나중에 UI 연결
    }
}

