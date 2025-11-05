using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class E_ModeStartBTN : MonoBehaviour
{
    [SerializeField] private TMP_InputField nameInputField;  // 이름 입력창
    [SerializeField] private GameObject nameCanvas;          // 캔버스 전체 오브젝트
    private const string PlayerNameKey = "PlayerName";
    public GameObject player;

    void Start()
    {
        Time.timeScale = 0f;
        player.SetActive(false);
    }

    public void StartButtonClick()
    {
        string playerName = nameInputField.text;

        if (string.IsNullOrWhiteSpace(playerName))
        {
            return;
        }

        // 이름 저장
        LocalScoreManager.Instance.playerName = playerName;
        Debug.Log("이름 저장 완료: " + playerName);

        // 캔버스 비활성화
        Time.timeScale = 1f;
        player.SetActive(true);
        nameCanvas.SetActive(false);
    }
}
