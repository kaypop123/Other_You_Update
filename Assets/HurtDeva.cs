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

    void FindDeathBackground()
    {
        GameObject backgroundObj = GameObject.Find("DeathBackground");
        if (backgroundObj != null)
            deathBackground = backgroundObj.GetComponent<SpriteRenderer>();
    }

    void FindCameraShake()
    {
        cameraShake = Camera.main != null ? Camera.main.GetComponent<CameraShakeSystem>() : null;
    }

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

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isParrying || isDead) return;

        EnemyMovement enemy = other.GetComponentInParent<EnemyMovement>();
        Arrow arrow = other.GetComponent<Arrow>();

        if (other.CompareTag("FireBall"))
        {
            Debug.Log("FireBall 충돌");
            Die();
        }

        if (other.CompareTag("EnemyAttack") || other.CompareTag("damageAmount"))
        {
            DebaraMovement movement = GetComponent<DebaraMovement>();
            if (movement != null && movement.isInvincible) return;

            EnemyDamageBumpAgainst bump = other.GetComponent<EnemyDamageBumpAgainst>();
            if (bump != null) bump.TriggerDamageCooldown(0.5f);

            int damage = 0;
            if (enemy != null) damage = enemy.GetDamage();
            else if (arrow != null) damage = arrow.damage;

            TakeDamage(damage);
            animator.Play("Hurt", 0, 0f);
            ShowBloodEffect();
            Knockback(other.transform);

            if (cameraShake != null)
                StartCoroutine(cameraShake.Shake(0.15f, 0.15f));
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        DevaStats.Instance.currentHealth -= damage;
        DevaStats.Instance.currentHealth = Mathf.Clamp(DevaStats.Instance.currentHealth, 0, DevaStats.Instance.maxHealth);

        DebaraMovement movement = GetComponent<DebaraMovement>();
        if (movement != null) movement.ForceEndAttack();

        if (healthBarUI != null)
            healthBarUI.UpdateHealthBar(DevaStats.Instance.currentHealth, true);

        if (charStateGUIEffect != null)
            charStateGUIEffect.TriggerHitEffect();

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

    private void Knockback(Transform enemyTransform)
    {
        if (rb == null) return;

        float direction = transform.position.x - enemyTransform.position.x > 0 ? 1f : -1f;
        rb.velocity = new Vector2(knockbackForce * direction, rb.velocity.y + 1f);
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        DisableControls();

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.simulated = false;
        }

        foreach (EnemySpawner spawner in FindObjectsOfType<EnemySpawner>())
            spawner.StopSpawning();

        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
            enemy.SetActive(false);

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

    public void RespawnDeva()
{
    // 게임 오브젝트 활성화 먼저
    gameObject.SetActive(true);

    // 컴포넌트 다시 가져오기 (씬 재로드 후 null일 수 있음)
    if (animator == null)
        animator = GetComponent<Animator>();
    if (rb == null)
        rb = GetComponent<Rigidbody2D>();
    if (spriteRenderer == null)
        spriteRenderer = GetComponent<SpriteRenderer>();

    // 사망 상태 해제
    isDead = false;

    // 애니메이터 초기화
    if (animator != null)
    {
        animator.ResetTrigger("Die");
        animator.Play("DevaIdle");
    }

    // Rigidbody 초기화
    if (rb != null)
    {
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.simulated = true;
        rb.velocity = Vector2.zero;
    }

    // 체력/에너지/마나 초기화
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
    if (movement != null)
        movement.enabled = true;

    MagicAttack magic = GetComponent<MagicAttack>();
    if (magic != null)
        magic.enabled = true;

    // SpriteRenderer 초기화
    if (spriteRenderer != null)
        spriteRenderer.sortingOrder = originalSortingOrder;

    // 사망 배경 초기화
    if (deathBackground != null)
    {
        Color color = deathBackground.color;
        color.a = 0f;
        deathBackground.color = color;
    }

    // 스폰 위치로 이동 (없으면 기본 위치)
    if (SpawnManager.Instance != null)
        transform.position = SpawnManager.Instance.spawnPosition;
    else
        transform.position = Vector3.zero;

    Debug.Log("[HurtDeva] 리스폰 완료!");
}


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

    private void ChangeLayerOnDeath()
    {
        if (spriteRenderer != null)
            spriteRenderer.sortingOrder = 11;
    }

    public bool IsDead()
    {
        return isDead;
    }

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

    /// <summary>
    /// 강제로 리스폰 (화면에 표시)
    /// </summary>
    public void ForceRespawn()
    {
        isDead = true;
        RespawnDeva();
    }

    /// <summary>
    /// 화면 안 나오게 상태만 초기화
    /// 씬 리스타트 시 사용
    /// </summary>
    public void ResetState()
    {
        isDead = false;

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.simulated = true;
        }

        if (DevaStats.Instance != null)
        {
            DevaStats.Instance.currentHealth = DevaStats.Instance.maxHealth;
            DevaStats.Instance.SetCurrentEnergy(DevaStats.Instance.maxEnergy);
            DevaStats.Instance.SetCurrentMana(DevaStats.Instance.maxMana);
        }

        UpdateHealthUI();

        if (deathBackground != null)
        {
            Color color = deathBackground.color;
            color.a = 0f;
            deathBackground.color = color;
        }

        if (animator != null)
        {
            animator.ResetTrigger("Die");
            animator.Play("DevaIdle");
        }

        DebaraMovement movement = GetComponent<DebaraMovement>();
        if (movement != null) movement.enabled = false;

        MagicAttack magic = GetComponent<MagicAttack>();
        if (magic != null) magic.enabled = false;

        gameObject.SetActive(false);

        Debug.Log("[HurtDeva] 상태 초기화 완료, 화면 비활성화");
    }

    
}
