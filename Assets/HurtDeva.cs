using System.Collections;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// 데바 캐릭터 피해 처리 및 사망/리스폰 관리
/// 데바가 공격받으면 체력 감소, 피격 효과, 넉백 처리
/// 체력이 0이 되면 사망 처리: 적 스폰 중단, 적 제거, UI 표시, 컨트롤 비활성화
/// </summary>
public class HurtDeva : MonoBehaviour
{
    // Animator
    private Animator animator;

    // 피격 효과 프리팹
    public GameObject[] bloodEffectPrefabs;
    public GameObject parringEffects; // 패링 효과
    public ParticleSystem bloodEffectParticle; // 파티클

    // 카메라 흔들림 시스템
    public CameraShakeSystem cameraShake;

    // Rigidbody2D, SpriteRenderer
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    // 넉백 힘
    public float knockbackForce = 5f;

    // 패링 상태
    private bool isParrying = false;

    [Header("Hit Effect Position")]
    public Transform pos; // 피격 효과 표시 위치

    // UI
    public DevaHealthBarUI healthBarUI;
    public CharStateGUIEffect charStateGUIEffect;

    // 사망 상태
    private bool isDead = false;

    [Header("Death Effect Elements")]
    public SpriteRenderer deathBackground; // 사망 시 배경 페이드용

    public static HurtDeva Instance; // 싱글톤
    private int originalSortingOrder; // SpriteRenderer 기본 Order in Layer 저장

    void Awake()
    {
        // 싱글톤 초기화
        if (Instance == null)
            Instance = this;
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalSortingOrder = spriteRenderer != null ? spriteRenderer.sortingOrder : 0;

        FindCameraShake();
        FindDeathBackground();

        // 체력 초기화
        DevaStats.Instance.currentHealth = DevaStats.Instance.maxHealth;
        if (healthBarUI != null)
            healthBarUI.Initialize(DevaStats.Instance.maxHealth);

        // 사망 배경 초기 투명 처리
        if (deathBackground != null)
        {
            Color startColor = deathBackground.color;
            startColor.a = 0f;
            deathBackground.color = startColor;
        }
    }

    void Update()
    {
        // 카메라 흔들기와 사망 배경이 없으면 계속 찾아줌
        if (cameraShake == null) FindCameraShake();
        if (deathBackground == null) FindDeathBackground();
    }

    // DeathBackground 찾아서 연결
    void FindDeathBackground()
    {
        GameObject backgroundObj = GameObject.Find("DeathBackground");
        if (backgroundObj != null)
            deathBackground = backgroundObj.GetComponent<SpriteRenderer>();
    }

    // CameraShakeSystem 찾아서 연결
    void FindCameraShake()
    {
        cameraShake = Camera.main != null ? Camera.main.GetComponent<CameraShakeSystem>() : null;
    }

    /// <summary>
    /// 피격 시 혈흔 효과 표시
    /// </summary>
    public void ShowBloodEffect()
    {
        if (bloodEffectPrefabs.Length > 0)
        {
            int index = Random.Range(0, bloodEffectPrefabs.Length);
            GameObject effect = Instantiate(bloodEffectPrefabs[index], pos.position, Quaternion.identity);
            Destroy(effect, 0.3f);

            if (bloodEffectParticle != null)
            {
                ParticleSystem particle = Instantiate(bloodEffectParticle, pos.position, Quaternion.identity);
                particle.Play();
                Destroy(particle.gameObject, particle.main.duration + 0.5f);
            }
        }
    }

    // 충돌 감지
    void OnTriggerEnter2D(Collider2D other)
    {
        // 패링 중이거나 이미 사망 상태면 무시
        if (isParrying || isDead) return;

        EnemyMovement enemy = other.GetComponentInParent<EnemyMovement>();
        Arrow arrow = other.GetComponent<Arrow>();

        // FireBall 충돌 시 즉사
        if (other.CompareTag("FireBall"))
        {
            Debug.Log("FireBall 충돌");
            Die();
        }

        // 적 공격에 맞음
        if (other.CompareTag("EnemyAttack") || other.CompareTag("damageAmount"))
        {
            DebaraMovement movement = GetComponent<DebaraMovement>();
            if (movement != null && movement.isInvincible) return;

            // 피해 쿨타임 처리
            EnemyDamageBumpAgainst bump = other.GetComponent<EnemyDamageBumpAgainst>();
            if (bump != null) bump.TriggerDamageCooldown(0.5f);

            int damage = 0;
            if (enemy != null) damage = enemy.GetDamage();
            else if (arrow != null) damage = arrow.damage;

            // 데미지 적용
            TakeDamage(damage);

            // 피격 애니메이션 재생
            animator.Play("Hurt", 0, 0f);

            // 혈흔 효과 표시
            ShowBloodEffect();

            // 넉백 처리
            Knockback(other.transform);

            // 카메라 흔들림
            if (cameraShake != null)
                StartCoroutine(cameraShake.Shake(0.15f, 0.15f));
        }
    }

    /// <summary>
    /// 데미지 적용
    /// </summary>
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        DevaStats.Instance.currentHealth -= damage;
        DevaStats.Instance.currentHealth = Mathf.Clamp(DevaStats.Instance.currentHealth, 0, DevaStats.Instance.maxHealth);

        // 공격 강제 종료
        DebaraMovement movement = GetComponent<DebaraMovement>();
        if (movement != null) movement.ForceEndAttack();

        // UI 갱신
        if (healthBarUI != null)
            healthBarUI.UpdateHealthBar(DevaStats.Instance.currentHealth, true);

        if (charStateGUIEffect != null)
            charStateGUIEffect.TriggerHitEffect();

        // 체력 0이면 사망
        if (DevaStats.Instance.currentHealth <= 0)
            Die();
    }

    public void UpdateHealthUI()
    {
        if (healthBarUI != null)
            healthBarUI.UpdateHealthBar(DevaStats.Instance.currentHealth, true);
    }

    public void CancelDamage()
    {
        animator.ResetTrigger("Hurt");
    }

    public void StartParry()
    {
        isParrying = true;
        StartCoroutine(ResetParry());
    }

    IEnumerator ResetParry()
    {
        yield return new WaitForSeconds(0.1f);
        isParrying = false;
    }

    // 넉백 처리
    private void Knockback(Transform enemyTransform)
    {
        if (rb == null) return;

        float direction = transform.position.x - enemyTransform.position.x > 0 ? 1f : -1f;
        rb.velocity = new Vector2(knockbackForce * direction, rb.velocity.y + 1f);
    }

    /// <summary>
    /// 사망 처리
    /// - 컨트롤 비활성화
    /// - Rigidbody 비활성화
    /// - EnemySpawner 중단
    /// - 기존 적 모두 화면에서 제거
    /// - 사망 애니메이션 재생
    /// - DeathPanel UI 표시
    /// </summary>
    private void Die()
    {
        if (isDead) return;
        isDead = true;

        // 플레이어 컨트롤 비활성화
        DisableControls();

        // Rigidbody 비활성화
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.simulated = false;
        }

        // 🔹 모든 EnemySpawner 스폰 중단
        foreach (EnemySpawner spawner in FindObjectsOfType<EnemySpawner>())
        {
            spawner.StopSpawning();
        }

        // 🔹 이미 생성된 Enemy 오브젝트 화면에서 제거
        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            enemy.SetActive(false); // 화면에서 완전히 사라짐
        }

        // 사망 애니메이션 및 DeathPanel UI
        if (deathBackground != null)
        {
            deathBackground.DOFade(1f, 0.5f).OnComplete(() =>
            {
                animator.SetTrigger("Die");
                ChangeLayerOnDeath();
                ShowDeathPanelUI();
            });
        }
        else
        {
            animator.SetTrigger("Die");
            ChangeLayerOnDeath();
            ShowDeathPanelUI();
        }
    }

    /// <summary>
    /// 데바 리스폰 처리
    /// - 체력/마나/에너지 초기화
    /// - Rigidbody 및 컨트롤 활성화
    /// - 사망 배경 초기화
    /// </summary>
    public void RespawnDeva()
    {
        if (!isDead) return;

        isDead = false;
        gameObject.SetActive(true);

        // 사망 애니메이션 초기화
        if (animator != null)
        {
            animator.ResetTrigger("Die");
            animator.Play("DevaIdle");
        }

        // Rigidbody 활성화
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.simulated = true;
            rb.velocity = Vector2.zero;
        }

        // 체력, 에너지, 마나 초기화
        if (DevaStats.Instance != null)
        {
            DevaStats.Instance.currentHealth = DevaStats.Instance.maxHealth;
            DevaStats.Instance.SetCurrentEnergy(DevaStats.Instance.maxEnergy);
            DevaStats.Instance.SetCurrentMana(DevaStats.Instance.maxMana);
        }

        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.currentHealth = PlayerStats.Instance.maxHealth;
            PlayerStats.Instance.SetCurrentEnergy(PlayerStats.Instance.maxEnergy);
            PlayerStats.Instance.SetCurrentMana(PlayerStats.Instance.maxMana);
            if (HurtPlayer.Instance != null)
                HurtPlayer.Instance.UpdateHealthUI();
        }

        // UI 갱신
        UpdateHealthUI();

        // 컨트롤 활성화
        DebaraMovement movement = GetComponent<DebaraMovement>();
        if (movement != null) movement.enabled = true;

        MagicAttack magic = GetComponent<MagicAttack>();
        if (magic != null) magic.enabled = true;

        // SpriteRenderer 초기화
        if (spriteRenderer != null)
            spriteRenderer.sortingOrder = 0;

        // 사망 배경 초기화
        if (deathBackground != null)
        {
            Color color = deathBackground.color;
            color.a = 0f;
            deathBackground.color = color;
        }

        // 스폰 위치로 이동
        if (SpawnManager.Instance != null)
        {
            transform.position = SpawnManager.Instance.spawnPosition;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = originalSortingOrder;
        }

        Debug.Log("[HurtDeva] 리스폰 완료!");
    }

    /// <summary>
    /// 플레이어/데바 컨트롤 비활성화
    /// </summary>
    private void DisableControls()
    {
        DebaraMovement movement = GetComponent<DebaraMovement>();
        if (movement != null)
        {
            if (movement.isInvincible) return;
            movement.enabled = false;
            movement.ForceEndAttack();
        }

        MagicAttack attack = GetComponent<MagicAttack>();
        if (attack != null) attack.enabled = false;
    }

    // SpriteRenderer Layer 변경
    private void ChangeLayerOnDeath()
    {
        if (spriteRenderer != null)
            spriteRenderer.sortingOrder = 11;
    }

    public bool IsDead()
    {
        return isDead;
    }

    // DeathPanel UI 표시
    private void ShowDeathPanelUI()
    {
        SceneUIManager sceneUIManager = FindObjectOfType<SceneUIManager>();
        if (sceneUIManager != null)
        {
            sceneUIManager.ShowManagedDeathPanel();
            Debug.Log("[HurtDeva] DeathPanel 표시 완료!");
        }
        else
        {
            Debug.LogError("[HurtDeva] SceneUIManager를 찾지 못해 DeathPanel 표시 실패!");
        }
    }
}
