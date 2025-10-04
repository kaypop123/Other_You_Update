using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class HurtPlayer : MonoBehaviour
{
    private Animator TestAnime;
    public GameObject[] bloodEffectPrefabs;
    public GameObject parringEffects;
    public ParticleSystem bloodEffectParticle;

    public CameraShakeSystem cameraShake;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    public int CurrentHealth => PlayerStats.Instance != null ? PlayerStats.Instance.currentHealth : 0;
    public int MaxHealth => PlayerStats.Instance != null ? PlayerStats.Instance.maxHealth : 100;

    public float knockbackForce = 5f;
    private bool isParrying = false;

    [Header("Hit Effect Position")]
    public Transform pos; // 피격 효과 위치 기준

    public HealthBarUI healthBarUI;
    public CharStateGUIEffect charStateGUIEffect;
    private bool isDead = false;

    [Header("Death Effect Elements")]
    public SpriteRenderer deathBackground; // 사망 시 배경 어둡게 처리
    public static HurtPlayer Instance;
    private int originalSortingOrder;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    void Start()
    {
        TestAnime = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        cameraShake = Camera.main != null ? Camera.main.GetComponent<CameraShakeSystem>() : null;

        if (cameraShake == null)
            Debug.LogWarning("CameraShakeSystem을 찾을 수 없습니다.");

        if (healthBarUI != null)
            healthBarUI.Initialize(MaxHealth);

        // 배경 초기화
        if (deathBackground != null)
        {
            Color startColor = deathBackground.color;
            startColor.a = 0f;
            deathBackground.color = startColor;
        }

        FindCameraShake();
        FindDeathBackground();

        originalSortingOrder = spriteRenderer != null ? spriteRenderer.sortingOrder : 0;
    }

    void Update()
    {
        if (cameraShake == null)
            FindCameraShake();

        if (deathBackground == null)
            FindDeathBackground();
    }

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

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isParrying || isDead) return;

        EnemyMovement enemy = other.GetComponentInParent<EnemyMovement>();
        Arrow enemyArrow = other.GetComponent<Arrow>();
        Thron thron = other.GetComponent<Thron>();

        if (other.CompareTag("FireBall"))
        {
            Debug.Log("FireBall에 피격됨");
            Die();
        }

        if (other.CompareTag("EnemyAttack") || other.CompareTag("damageAmount"))
        {
            AdamMovement playerMovement = GetComponent<AdamMovement>();
            AdamUltimateSkill ultimateSkill = GetComponent<AdamUltimateSkill>();

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

            TakeDamage(damage);
            TestAnime.Play("Hurt", 0, 0f);
            ShowBloodEffect();
            Knockback(other.transform);

            if (cameraShake != null)
                StartCoroutine(cameraShake.Shake(0.15f, 0.15f));
        }
    }

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

    private void Die()
{
    if (isDead) return;
    isDead = true;

    Debug.Log($"{gameObject.name} 사망!");

    // 🔹 적 스폰 중단
    foreach (EnemySpawner spawner in FindObjectsOfType<EnemySpawner>())
    {
        spawner.StopSpawning();
    }

    // 🔹 이미 생성된 적 비활성화
    foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
    {
        enemy.SetActive(false);
    }

    // 아담 이동/공격만 비활성화
    AdamMovement movement = GetComponent<AdamMovement>();
    if (movement != null) movement.enabled = false;

    CharacterAttack attack = GetComponent<CharacterAttack>();
    if (attack != null) attack.enabled = false;

    if (rb != null)
    {
        rb.velocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.simulated = false;
    }

    if (deathBackground != null)
    {
        deathBackground.DOFade(1f, 0.5f).OnComplete(() =>
        {
            TestAnime.SetTrigger("Die");
            ChangeLayerOnDeath();

            // UI 표시
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

    public void RespawnPlayer()
    {
        if (!isDead) return;

        isDead = false;
        gameObject.SetActive(true);

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.simulated = true;
            rb.velocity = Vector2.zero;
        }

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

        if (TestAnime != null)
        {
            TestAnime.ResetTrigger("Die");
            TestAnime.Play("Idle");
        }

        EnablePlayerControls();

        if (spriteRenderer != null)
            spriteRenderer.sortingOrder = originalSortingOrder;

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

    public bool IsDead()
    {
        return isDead;
    }

    private IEnumerator DisableAfterDeath()
    {
        yield return new WaitForSeconds(5f);
    }
}
