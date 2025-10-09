using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class HurtPlayer : MonoBehaviour
{
    // ------------------- 컴포넌트 참조 -------------------
    private Animator TestAnime; // 플레이어 애니메이터
    private Rigidbody2D rb; // Rigidbody2D
    private SpriteRenderer spriteRenderer; // SpriteRenderer
    public CameraShakeSystem cameraShake; // 카메라 흔들기

    // ------------------- 피격 효과 -------------------
    public GameObject[] bloodEffectPrefabs; // 피격 시 혈흔 이펙트 배열
    public ParticleSystem bloodEffectParticle; // 피격 파티클
    public GameObject parringEffects; // 패링 효과
    [Header("Hit Effect Position")]
    public Transform pos; // 피격 효과 위치 기준

    // ------------------- 플레이어 상태 -------------------
    public int CurrentHealth => PlayerStats.Instance != null ? PlayerStats.Instance.currentHealth : 0;
    public int MaxHealth => PlayerStats.Instance != null ? PlayerStats.Instance.maxHealth : 100;
    public float knockbackForce = 5f; // 넉백 힘
    private bool isParrying = false; // 패링 상태 여부
    private bool isDead = false; // 사망 상태 여부

    // ------------------- UI 및 GUI -------------------
    public HealthBarUI healthBarUI; // 체력바 UI
    public CharStateGUIEffect charStateGUIEffect; // 상태 GUI 효과
    [Header("Death Effect Elements")]
    public SpriteRenderer deathBackground; // 사망 시 배경 어둡게 처리

    // ------------------- 리스폰 관련 -------------------
    public static HurtPlayer Instance; // 싱글톤
    public Transform respawnPoint; // 에디터에서 지정한 리스폰 위치
    private Vector3 startPosition; // 게임 시작 위치 저장
    private int originalSortingOrder; // 사망 전 sprite sortingOrder

    void Awake()
    {
        // 싱글톤 초기화
        if (Instance == null)
            Instance = this;
    }

    void Start()
    {
        // 컴포넌트 초기화
        TestAnime = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        cameraShake = Camera.main != null ? Camera.main.GetComponent<CameraShakeSystem>() : null;

        if (cameraShake == null)
            Debug.LogWarning("CameraShakeSystem을 찾을 수 없습니다.");

        if (healthBarUI != null)
            healthBarUI.Initialize(MaxHealth); // 체력 UI 초기화

        // 사망 배경 초기화 (투명)
        if (deathBackground != null)
        {
            Color startColor = deathBackground.color;
            startColor.a = 0f;
            deathBackground.color = startColor;
        }

        // 기타 컴포넌트 참조 연결
        FindCameraShake();
        FindDeathBackground();

        // 원래 SpriteRenderer sortingOrder 저장
        originalSortingOrder = spriteRenderer != null ? spriteRenderer.sortingOrder : 0;

        // 🔹 게임 시작 위치 저장 (리스폰 시 사용)
        startPosition = transform.position;
    }

    void Update()
    {
        // 참조가 끊어진 경우 다시 찾기
        if (cameraShake == null)
            FindCameraShake();

        if (deathBackground == null)
            FindDeathBackground();
    }

    // ------------------- 참조 찾기 -------------------
    void FindDeathBackground()
    {
        GameObject backgroundObj = GameObject.Find("DeathBackground");
        if (backgroundObj != null)
            deathBackground = backgroundObj.GetComponent<SpriteRenderer>();
        else
            Debug.LogWarning("DeathBackground 오브젝트를 찾을 수 없습니다.");
    }

    void FindCameraShake()
    {
        cameraShake = Camera.main != null ? Camera.main.GetComponent<CameraShakeSystem>() : null;
        if (cameraShake == null)
            Debug.LogWarning("CameraShakeSystem을 찾을 수 없습니다.");
    }

    // ------------------- 피격 효과 -------------------
    public void ShowBloodEffect()
    {
        if (bloodEffectPrefabs != null && bloodEffectPrefabs.Length > 0)
        {
            int randomIndex = Random.Range(0, bloodEffectPrefabs.Length);
            GameObject selectedEffect = bloodEffectPrefabs[randomIndex];

            GameObject bloodEffect = Instantiate(selectedEffect, pos.position, Quaternion.identity);
            Destroy(bloodEffect, 0.3f);

            if (bloodEffectParticle != null)
            {
                ParticleSystem bloodParticle = Instantiate(bloodEffectParticle, pos.position, Quaternion.identity);
                bloodParticle.Play();
                Destroy(bloodParticle.gameObject, bloodParticle.main.duration + 0.5f);
            }
        }
        else
        {
            Debug.LogWarning("bloodEffectPrefabs 배열이 비어 있습니다!");
        }
    }

    // ------------------- 충돌 처리 -------------------
    void OnTriggerEnter2D(Collider2D other)
    {
        if (isParrying || isDead) return;

        EnemyMovement enemy = other.GetComponentInParent<EnemyMovement>();
        Arrow enemyArrow = other.GetComponent<Arrow>();
        Thron thron = other.GetComponent<Thron>();

        // 즉사 공격 처리
        if (other.CompareTag("FireBall"))
        {
            Debug.Log("FireBall에 피격됨");
            Die();
        }

        // 일반 공격 처리
        if (other.CompareTag("EnemyAttack") || other.CompareTag("damageAmount"))
        {
            AdamMovement playerMovement = GetComponent<AdamMovement>();
            AdamUltimateSkill ultimateSkill = GetComponent<AdamUltimateSkill>();

            // 무적 상태이면 피해 무효화
            if ((playerMovement != null && playerMovement.isInvincible) ||
                (ultimateSkill != null && ultimateSkill.isCasting))
            {
                Debug.Log("무적 상태 - 피해 무효화");
                return;
            }

            EnemyDamageBumpAgainst damageTrigger = other.GetComponent<EnemyDamageBumpAgainst>();
            if (damageTrigger != null)
                damageTrigger.TriggerDamageCooldown(0.5f);

            int damage = 0;
            if (enemy != null)
                damage = enemy.GetDamage();
            else if (enemyArrow != null)
                damage = enemyArrow.damage;
            else if (thron != null)
                damage = thron.damage;

            // 데미지 적용 및 피격 효과
            TakeDamage(damage);
            TestAnime.Play("Hurt", 0, 0f);
            ShowBloodEffect();
            Knockback(other.transform);

            if (cameraShake != null)
                StartCoroutine(cameraShake.Shake(0.15f, 0.15f));
        }
    }

    // ------------------- 데미지 처리 -------------------
    public void TakeDamage(int damage)
    {
        if (isDead || PlayerStats.Instance == null) return;

        PlayerStats.Instance.currentHealth -= damage;
        PlayerStats.Instance.currentHealth = Mathf.Clamp(PlayerStats.Instance.currentHealth, 0, PlayerStats.Instance.maxHealth);

        Debug.Log($"[HurtPlayer] 체력: {PlayerStats.Instance.currentHealth} / {PlayerStats.Instance.maxHealth}");

        if (healthBarUI != null)
            healthBarUI.UpdateHealthBar(PlayerStats.Instance.currentHealth, true);

        if (charStateGUIEffect != null)
            charStateGUIEffect.TriggerHitEffect();

        if (PlayerStats.Instance.currentHealth <= 0)
            Die();
    }

    public void UpdateHealthUI()
    {
        if (healthBarUI != null)
            healthBarUI.UpdateHealthBar(PlayerStats.Instance.currentHealth, true);
    }

    public void CancelDamage()
    {
        Debug.Log("패링 중 피해 무효화");
        TestAnime.ResetTrigger("Hurt");
    }

    public void StartParry()
    {
        isParrying = true;
        StartCoroutine(ResetParry());
    }

    private IEnumerator ResetParry()
    {
        yield return new WaitForSeconds(0.1f);
        isParrying = false;
    }

    private void Knockback(Transform target)
    {
        if (rb == null) return;
        float direction = transform.position.x - target.position.x > 0 ? 1f : -1f;
        rb.velocity = new Vector2(knockbackForce * direction, rb.velocity.y + 1f);
    }

    // ------------------- 사망 처리 -------------------
    private void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log($"{gameObject.name} 사망!");

        // 🔹 적 스폰 중단
        foreach (EnemySpawner spawner in FindObjectsOfType<EnemySpawner>())
            spawner.StopSpawning();

        // 🔹 이미 생성된 적 비활성화
        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
            enemy.SetActive(false);

        // 플레이어 이동/공격 비활성화
        DisablePlayerControls();

        // Rigidbody 처리
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.simulated = false;
        }

        // 사망 배경 처리 후 애니메이션
        if (deathBackground != null)
        {
            deathBackground.DOFade(1f, 0.5f).OnComplete(() =>
            {
                TestAnime.SetTrigger("Die");
                ChangeLayerOnDeath();
                ShowDeathPanelUI();
            });
        }
        else
        {
            TestAnime.SetTrigger("Die");
            ChangeLayerOnDeath();
            ShowDeathPanelUI();
        }
    }

    private void ShowDeathPanelUI()
    {
        SceneUIManager sceneUIManager = FindObjectOfType<SceneUIManager>();

        if (sceneUIManager != null)
        {
            sceneUIManager.ShowManagedDeathPanel();
            Debug.Log("[HurtPlayer] DeathPanel 표시 완료!");
        }
        else
        {
            Debug.LogError("[HurtPlayer] SceneUIManager를 찾을 수 없어 DeathPanel 표시 실패.");
        }
    }

    private void DisablePlayerControls()
    {
        AdamMovement movement = GetComponent<AdamMovement>();
        if (movement != null) movement.enabled = false;

        CharacterAttack attack = GetComponent<CharacterAttack>();
        if (attack != null) attack.enabled = false;

        Debug.Log("플레이어 컨트롤 비활성화 완료");
    }

    private void ChangeLayerOnDeath()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = 11;
            Debug.Log($"[HurtPlayer] Order in Layer 변경: {spriteRenderer.sortingOrder}");
        }
    }

    // ------------------- 리스폰 처리 -------------------
    public void RespawnPlayer()
    {
        if (!isDead) return;

        isDead = false;
        gameObject.SetActive(true);

        // 🔹 리스폰 위치 결정
        Vector3 respawnPos = respawnPoint != null ? respawnPoint.position : startPosition;
        transform.position = respawnPos;

        if (respawnPoint == null)
            Debug.LogWarning("[HurtPlayer] RespawnPoint 미지정, 시작 위치로 리스폰");

        // Rigidbody 초기화
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.simulated = true;
            rb.velocity = Vector2.zero;
        }

        // 스탯 초기화
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.currentHealth = PlayerStats.Instance.maxHealth;
            PlayerStats.Instance.SetCurrentEnergy(PlayerStats.Instance.maxEnergy);
            PlayerStats.Instance.SetCurrentMana(PlayerStats.Instance.maxMana);
        }

        if (DevaStats.Instance != null)
        {
            DevaStats.Instance.currentHealth = DevaStats.Instance.maxHealth;
            DevaStats.Instance.SetCurrentEnergy(DevaStats.Instance.maxEnergy);
            DevaStats.Instance.SetCurrentMana(DevaStats.Instance.maxMana);

            if (HurtDeva.Instance != null)
                HurtDeva.Instance.UpdateHealthUI();
        }

        UpdateHealthUI();

        // 애니메이션 초기화
        if (TestAnime != null)
        {
            TestAnime.ResetTrigger("Die");
            TestAnime.Play("Idle");
        }

        EnablePlayerControls();

        if (spriteRenderer != null)
            spriteRenderer.sortingOrder = originalSortingOrder;

        // 사망 배경 초기화
        if (deathBackground != null)
        {
            Color color = deathBackground.color;
            color.a = 0f;
            deathBackground.color = color;
        }

        Debug.Log("[HurtPlayer] 플레이어 리스폰 완료!");
    }

    private void EnablePlayerControls()
    {
        AdamMovement movement = GetComponent<AdamMovement>();
        if (movement != null) movement.enabled = true;

        CharacterAttack attack = GetComponent<CharacterAttack>();
        if (attack != null) attack.enabled = true;

        Debug.Log("플레이어 컨트롤 재활성화 완료");
    }

    public bool IsDead() => isDead;
}
