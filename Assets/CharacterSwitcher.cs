using System.Collections;
using UnityEngine;

public class CharacterSwitcher : MonoBehaviour
{
    public GameObject adamObject;
    public GameObject debaObject;

    public Animator switchAnimator;
    public GameObject switchEffectObject;

    public CharacterGUIController guiController;

    [Header("Switch Settings")]
    public string triggerAdamToDeba = "Play_AdamToDeba";
    public string triggerDebaToAdam = "Play_DebaToAdam";
    public float switchDelay = 0.7f;  // 애니메이션 재생 시간
    public float switchCooldown = 1.5f; // 쿨타임

    private bool isAdamActive = true;
    private bool canSwitch = true;

    void Start()
    {
        ActivateAdamImmediate();
    }

    void LateUpdate()
    {
        bool isAdamDead = HurtPlayer.Instance != null && HurtPlayer.Instance.IsDead();
        bool isDebaDead = HurtDeva.Instance != null && HurtDeva.Instance.IsDead();

        // Adam 궁극기
        var adamUlt = adamObject?.GetComponent<AdamUltimateSkill>();
        bool adamUsingUltimate = adamUlt != null && adamUlt.isCasting && adamObject.activeInHierarchy;

        // Adam 블레이드(또는 다른 스킬) 캐스팅
        var adamCast = adamObject?.GetComponent<AdamIsCasting>(); 
        bool adamUsingBlade = adamCast != null && adamCast.IsCasting && adamObject.activeInHierarchy;

        // Deva 궁극기 / 레이저 캐스팅
        var devaCast = debaObject?.GetComponent<DevaCastingBool>();
        bool devaUsingUltimate = devaCast != null && devaCast.IsCasting && debaObject.activeInHierarchy;

        //  셋 중 하나라도 캐스팅 중이면 스위치 금지
        if (adamUsingUltimate || devaUsingUltimate || adamUsingBlade)
            return;

        if (Input.GetKeyDown(KeyCode.Tab) && canSwitch && !isAdamDead && !isDebaDead)
        {
            StartCoroutine(PlaySwitchSequence());
        }
    }



    IEnumerator PlaySwitchSequence()
    {
        canSwitch = false;

        Vector3 currentPos;

        if (isAdamActive)
        {
            currentPos = adamObject.transform.position;
            DeactivateAdam();
            adamObject.SetActive(false);
        }
        else
        {
            currentPos = debaObject.transform.position;
            DeactivateDeba();
            debaObject.SetActive(false);
        }

        // 스위치 이펙트 위치 설정
        switchEffectObject.transform.position = currentPos;

        // 이펙트 애니메이션 시작
        if (switchAnimator != null)
        {
            switchEffectObject.SetActive(true);

            string triggerToUse = isAdamActive ? triggerAdamToDeba : triggerDebaToAdam;
            switchAnimator.SetTrigger(triggerToUse);

            //   GUI 전환도 이펙트와 동시에 실행
            if (guiController != null)
            {
                if (isAdamActive)
                    guiController.SwitchToDeba();  // Deba로 바뀌는 이펙트니까 Deba GUI 활성화
                else
                    guiController.SwitchToAdam();
            }
        }

        yield return new WaitForSeconds(switchDelay);

        // 캐릭터 활성화는 딜레이 후 실행
        if (isAdamActive)
            ActivateDebaDelayed();
        else
            ActivateAdamDelayed();

        switchEffectObject.SetActive(false);

        yield return new WaitForSeconds(switchCooldown - switchDelay);
        canSwitch = true;
    }


    void ActivateAdamImmediate()
    {
        isAdamActive = true;
        adamObject.SetActive(true);
        debaObject.SetActive(false);
        guiController?.SwitchToAdam();
    }

    void ActivateDebaDelayed()
    {
        isAdamActive = false;
        debaObject.transform.position = switchEffectObject.transform.position;
        debaObject.SetActive(true);
        // 스위치 직후 상태 초기화
        var debaMovement = debaObject.GetComponent<DebaraMovement>();
        if (debaMovement != null)
        {
            debaMovement.ResetState(); 
        }

   

    }

    void ActivateAdamDelayed()
    {
        isAdamActive = true;
        adamObject.transform.position = switchEffectObject.transform.position;
        adamObject.SetActive(true);
      
    }

    void DeactivateAdam()
    {
        var adamMovement = adamObject.GetComponent<AdamMovement>();
        var ultimateSkill = adamObject.GetComponent<AdamUltimateSkill>(); 
        var JumpAttack = adamObject.GetComponent<JumpAttack>();
        if (adamMovement != null)
        {
            adamMovement.ForceStopDash();

            var buff = adamObject.GetComponent<AdamAttackSpeedBuff>();
            buff?.ResetSkillState();

            var bladeSkill = adamObject.GetComponentInChildren<BladeExhaustSkill>();
            bladeSkill?.ResetSkillState();

            Rigidbody2D rb = adamObject.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
        }
        if (JumpAttack != null)
        {
            JumpAttack.ResetJumpAttackState();
        }
        if (ultimateSkill != null)
        {
            ultimateSkill.CancelUltimate();
        }

        adamObject.SetActive(false);
    }
    void DeactivateDeba()
    {
        var debaMovement = debaObject.GetComponent<DebaraMovement>();
        var debaCs = debaObject.GetComponent<DevaContinuousSkill>();
        if (debaMovement != null)
        {
            debaMovement.ForceEndAttack();
            debaMovement.ForceCancelTeleport();
            debaMovement.ResetLaserSkill();
            debaCs.ResetSkillState();
             //  Deva의 Rigidbody 초기화
             Rigidbody2D rb = debaObject.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
        }

        debaObject.SetActive(false);
    }

}
