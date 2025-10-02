using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    public static int baseHP = 10;
    public static int baseAttack = 2;
    public static int level = 1;

    public int hp;
    public int attack;

    private void Start()
    {
        hp = baseHP;
        attack = baseAttack;
    }

    public static void LevelUp()
    {
        level++;
        baseHP += 5;
        baseAttack += 1;
        Debug.Log("적 레벨업! HP: " + baseHP + " / ATK: " + baseAttack);
    }

    public void TakeDamage(int amount)
    {
        hp -= amount;
        if (hp <= 0)
        {
            ScoreManager.instance.AddScore(1); // 처치 시 점수 증가
            Destroy(gameObject);
        }
    }
}
