using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAttackHitbox : MonoBehaviour
{
    public int damage = 10; // 보스의 공격력
    public bool fromAdam = false;
    public bool fromDeba = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 아담
        if (other.CompareTag("Player"))
        {
            AdamMovement adamMovement = other.GetComponent<AdamMovement>();
            HurtPlayer hurtPlayer = other.GetComponent<HurtPlayer>();
            if (hurtPlayer != null)
            {
                if (adamMovement.isInvincible)
                {
                    return;
                }
                hurtPlayer.TakeDamage(damage);
                Debug.Log("보스가 Player에게 데미지!");
            }
        }

        // 데바
        else if (other.CompareTag("DevaPlayer"))
        {
            HurtDeva hurtDeva = other.GetComponent<HurtDeva>();
            DebaraMovement devaMovement = other.GetComponent<DebaraMovement>();

            if (hurtDeva != null && devaMovement != null)
            {
                if (devaMovement.isInvincible)
                {
                    return;
                }

                hurtDeva.TakeDamage(damage);
                Debug.Log("보스가 DevaPlayer에게 데미지!");
            }
        }
    }
}

