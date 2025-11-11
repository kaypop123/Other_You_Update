using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
public class DebaraMovement : MonoBehaviour
{
    Rigidbody2D DebaraRigidbody;
    Animator DebaraAnime;
    SpriteRenderer DebaraSprite;

    public float JumpPower = 3f;
    public float MoveSpeed = 3f;
    private MagicAttack magicAttack; 
    private DevaEnergyBarUI DevaEnergyBarUI; 

    private bool canTeleport = true;
    private bool isTeleporting;
    private bool attackInputRecently = false;

    [SerializeField]
    private float teleportDistance = 5f;
    private float teleportCooldown = 1.0f;
    [SerializeField] private TrailRenderer teleportTrail;

    [SerializeField] private float teleportEnergyCost = 20f;
    [SerializeField] private float attackInputCooldown = 0.2f;

    public bool lastKeyWasRight = true;


    public bool isGround;
    public Transform JumpPos;
    public float checkRadiusJump;
    public LayerMask isLayer;

    public bool isAttacking = false;
    public bool isInvincible { get; private set; }
    [SerializeField] private Transform jumpAttackCheckPos; 
    [SerializeField] private float jumpAttackRayLength = 1.5f; 
    [SerializeField] private LayerMask jumpAttackBlockLayer; 
    private Vector2? pendingTeleportTarget = null;
    public bool isControllable = true;

    private DownJump currentPlatformComponent;
    private Transform currentPlatformPosition;
    private Vector3 lastPlatformPos;
    void Start()
    {
        DebaraRigidbody = GetComponent<Rigidbody2D>();
        DebaraAnime = GetComponent<Animator>();
        DebaraSprite = GetComponent<SpriteRenderer>();
        magicAttack = GetComponent<MagicAttack>();
        DevaEnergyBarUI = FindObjectOfType<DevaEnergyBarUI>(); // EnergyBarUI 
    }

    void Update()
    {
        if (!isControllable) return;
        AnimatorStateInfo currentState = DebaraAnime.GetCurrentAnimatorStateInfo(0);

        bool isInAttackAnimation = currentState.IsName("Cast1") || currentState.IsName("Cast2");

        if (isInAttackAnimation && isAttacking) 
        {
            StopMovement();
            return;
        }

        HandleAttack();

        if (!isTeleporting && !attackInputRecently)
        {
            HandleMovement();
        }

        if (!isInAttackAnimation && !attackInputRecently)
        {
            HandleTeleport();
        }

        HandleJump();
        DebaraAnimation();
        HandleFlip();
        HandleFall();
        FollowPlatform();
        if (Input.GetKeyDown(KeyCode.X) && !isAttacking)
        {
            CastLaserSkill();
        }
        DebaraAnime.SetBool("isGrounded", isGround);

        
    }

    float currentSpeed = 0f;
    float acceleration = 8f;
    float deceleration = 12f;
    public float maxSpeed = 4f;

    void HandleMovement()
    {
        if (isAttacking) return; 

        float hor = Input.GetAxisRaw("Horizontal");

        if (hor != 0)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, hor * maxSpeed, Time.deltaTime * acceleration);
        }
        else
        {
            currentSpeed = Mathf.Lerp(currentSpeed, 0, Time.deltaTime * (deceleration * 2f));
        }

        DebaraRigidbody.velocity = new Vector2(currentSpeed, DebaraRigidbody.velocity.y);
    }
    bool IsJumpAttackBlocked()
    {
        if (jumpAttackCheckPos == null) return false;

        RaycastHit2D hit = Physics2D.Raycast(jumpAttackCheckPos.position, Vector2.down, jumpAttackRayLength, jumpAttackBlockLayer);
        return hit.collider != null;
    }
    void HandleAttack()
    {
        if (isAttacking) return;

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            attackInputRecently = true;
            isAttacking = true;
            StartCoroutine(ResetAttackInputCooldown());

            if (magicAttack != null)
            {
                if (!isGround)
                {
                    if (IsJumpAttackBlocked())
                    {
                        isAttacking = false;
                        attackInputRecently = false;
                        Debug.Log("Jump Attack Blocked by Raycast!");
                        return;
                    }

                    DebaraAnime.Play("JumpAttack", 0, 0);
                }
                else
                {
                    DebaraAnime.Play("Attack", 0, 0);
                    StopMovement(); 
                }
            }
        }
    }



    public void EndAttack()
    {
        isAttacking = false; 
    }
    void HandleTeleport()
    {
        if (isTeleporting || !canTeleport || attackInputRecently || !isGround)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            float currentEnergy = DevaEnergyBarUI != null ? DevaEnergyBarUI.GetCurrentEnergy() : 0f;


            if (currentEnergy < teleportEnergyCost && DevaEnergyBarUI != null)
            {
                Debug.Log("�ڷ���Ʈ �Ұ�: ENERGY ����!");
                DevaEnergyBarUI.FlashBorder(); 
                return;
            }

            StartCoroutine(Teleport());
        }
    }
    public GameObject teleportStartEffectPrefab; 
    public GameObject teleportEndEffectPrefab; 
    public Transform teleportStartEffectPosition; 
    public Transform teleportEndEffectPosition;

    private IEnumerator Teleport()
    {
        if (DevaEnergyBarUI != null)
        {
            DevaEnergyBarUI.ReduceEnergy(teleportEnergyCost);
        }

        canTeleport = false;
        isTeleporting = true;
        isInvincible = true;

        if (teleportTrail != null)
            teleportTrail.emitting = true;

        float teleportDirection = DebaraSprite.flipX ? -1f : 1f;
        Vector2 direction = new Vector2(teleportDirection, 0);
        float distance = teleportDistance;


        Vector2 origin = (Vector2)transform.position + new Vector2(0, 0.5f) + direction * 0.3f;
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, distance, isLayer);


        Debug.DrawRay(origin, direction * distance, Color.red, 1f);




        Vector2 targetPosition;

        if (hit.collider != null)
        {
            float safeDistance = Mathf.Max(0f, hit.distance - 0.05f);
            targetPosition = (Vector2)transform.position + direction * safeDistance;
           
        }
        else
        {
            targetPosition = (Vector2)transform.position + direction * distance;

        }

        pendingTeleportTarget = targetPosition;

    
        if (teleportStartEffectPrefab != null && teleportStartEffectPosition != null)
        {
            GameObject startEffect = Instantiate(teleportStartEffectPrefab, teleportStartEffectPosition.position, Quaternion.identity);
            Destroy(startEffect, 0.3f);
        }

        yield return new WaitForSeconds(0.2f);

        transform.position = targetPosition;
        pendingTeleportTarget = null;


        if (teleportEndEffectPrefab != null && teleportEndEffectPosition != null)
        {
            GameObject endEffect = Instantiate(teleportEndEffectPrefab, teleportEndEffectPosition.position, Quaternion.identity);
            Destroy(endEffect, 0.5f);
        }

        if (teleportTrail != null)
            teleportTrail.emitting = false;

        isTeleporting = false;
        isInvincible = false;

        yield return new WaitForSeconds(teleportCooldown);
        canTeleport = true;
    }



    private IEnumerator ResetAttackInputCooldown()
    {
        yield return new WaitForSeconds(attackInputCooldown);
        attackInputRecently = false;
    }

    public void StopMovement()
    {
        DebaraRigidbody.velocity = Vector2.zero;
        currentSpeed = 0f;
    }

    void DebaraAnimation()
    {
        float hor = Input.GetAxisRaw("Horizontal");
        DebaraAnime.SetBool("run", Mathf.Abs(hor) > 0.00f);
        bool rising = DebaraRigidbody.velocity.y > 0.1f && !isGround && !(DebaraRigidbody.velocity.y < 0);
        DebaraAnime.SetBool("isJumping", rising);
    }

    void HandleFlip()
    {
        if (isAttacking) return; 

        float hor = Input.GetAxisRaw("Horizontal");
        if (hor != 0)
        {
            DebaraSprite.flipX = hor < 0;
        }
    }

    void HandleJump()
    {
        isGround = Physics2D.OverlapCircle(JumpPos.position, checkRadiusJump, isLayer);
        AnimatorStateInfo currentState = DebaraAnime.GetCurrentAnimatorStateInfo(0);
        bool isJumping = currentState.IsName("Jump");


        if (isAttacking)
        {
            return;
        }


        if (currentState.IsName("Attack"))
        {
            return;
        }

        if (isTeleporting || (!isGround && DebaraRigidbody.velocity.y < 0))
        {
            return;
        }

        if (Input.GetKey(KeyCode.DownArrow) && Input.GetKeyDown(KeyCode.LeftAlt))
        {
            if (currentPlatformComponent != null)
            {
                Debug.Log("플레이어가 밟은 발판 작동!");
                isGround = false;
                currentPlatformComponent.TriggerDownJump();  // 발판에서 실행할 함수 호출
            }
        }

        if (Input.GetKeyDown(KeyCode.LeftAlt) && isGround && !isJumping)
        {
            Debug.Log("Jumping...");
            DebaraRigidbody.velocity = new Vector2(DebaraRigidbody.velocity.x, JumpPower);
        }

        else if (IsEndlessModeScene())
        {
            if ((Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.UpArrow)) && isGround && !isJumping)
            {
                Debug.Log("Jumping...");
                DebaraRigidbody.velocity = new Vector2(DebaraRigidbody.velocity.x, JumpPower);
            }
        }

    }


    private bool IsEndlessModeScene()
    {
        // 씬 이름 정확히 "EndlessMode"로 맞춰줘
        return SceneManager.GetActiveScene().name == "EndlessMode_New";
    }
    void FollowPlatform()
    {
        if (currentPlatformPosition != null)
        {
            Debug.Log("추적중" + (currentPlatformPosition.position - lastPlatformPos));
            Vector3 platformDelta = currentPlatformPosition.position - lastPlatformPos;
            transform.position += platformDelta; // 발판이 움직인 만큼 따라감
            lastPlatformPos = currentPlatformPosition.position;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            Debug.Log("발판감지:" + collision.gameObject);
            currentPlatformComponent = collision.gameObject.GetComponent<DownJump>();
            currentPlatformPosition = collision.transform;
            lastPlatformPos = currentPlatformPosition.position;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            currentPlatformComponent = null;
            currentPlatformPosition = null;
        }
    }

    void HandleFall()
    {
        if (!isGround && DebaraRigidbody.velocity.y < 0)
        {
            DebaraAnime.SetBool("Fall", true);
        }
        else
        {
            DebaraAnime.SetBool("Fall", false);
        }
    }

    private void OnDrawGizmos()
    {
        if (JumpPos != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(JumpPos.position, checkRadiusJump);
        }

        if (jumpAttackCheckPos != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(jumpAttackCheckPos.position, jumpAttackCheckPos.position + Vector3.down * jumpAttackRayLength);
        }
    }
    public void ResetState()
    {
        isAttacking = false;
        attackInputRecently = false;
        isTeleporting = false;
        isInvincible = false;
        canTeleport = true;
        currentSpeed = 0f;

        if (DebaraAnime != null)
        {
            DebaraAnime.ResetTrigger("Attack");
            DebaraAnime.SetBool("run", false);
            DebaraAnime.SetBool("isJumping", false);
            DebaraAnime.SetBool("Fall", false);
        }


        if (DebaraRigidbody != null)
        {
            DebaraRigidbody.velocity = Vector2.zero;
        }
    }
    public void ForceEndAttack()
    {
        isAttacking = false;
        attackInputRecently = false;

        if (DebaraAnime != null && gameObject.activeInHierarchy)
        {
            DebaraAnime.ResetTrigger("Attack");
            DebaraAnime.Play("DevaIdle"); 
        }

        if (magicAttack != null)
        {
            magicAttack.EndAttacks();
        }
    }
    public void ForceCancelTeleport()
    {
        if (isTeleporting)
        {
            isTeleporting = false;
            canTeleport = true;
            isInvincible = false;

            if (teleportTrail != null)
            {
                teleportTrail.emitting = false;
            }


            if (pendingTeleportTarget.HasValue)
            {
                transform.position = pendingTeleportTarget.Value;
                pendingTeleportTarget = null;
            }

            StopAllCoroutines();
        }
    }
    [SerializeField] private GameObject laserPrefab;
    [SerializeField] private Transform laserSpawnOrigin; 

    [SerializeField] private int laserCount = 6; 
    [SerializeField] private float interval = 0.1f; 
    [SerializeField] private float spawnDistanceStep = 0.5f; 

    [SerializeField] private float laserManaCost = 40f; 

    [Header("UI")]
    public SkillCooldownUI laserCooldownUI; 


    public float laserCooldown = 6f; 
    private bool isLaserOnCooldown = false;
    private float laserCooldownEndTime = 0f;



    public void CastLaserSkill()
    {
        
        if (Time.time < laserCooldownEndTime)
        {
            return;
        }

        if (!isGround)
        {
            return;
        }

        if (!DevaStats.Instance.HasEnoughMana((int)laserManaCost))
        {
            if (DevaManaBarUI.Instance != null)
                DevaManaBarUI.Instance.FlashBorder();
            return;
        }


        DevaStats.Instance.ReduceMana((int)laserManaCost);


        laserCooldownEndTime = Time.time + laserCooldown;

        if (laserCooldownUI != null)
        {
            laserCooldownUI.cooldownTime = laserCooldown;
            laserCooldownUI.StartCooldown(); 
        }

        DebaraAnime.Play("Cast1");
        isAttacking = true;
    }




    [SerializeField] private float offsetX = 0.5f;
    [SerializeField] private float offsetY = 0f;

    private IEnumerator SpawnLaserSequence()
    {
        isAttacking = true;

        Vector3 direction = DebaraSprite.flipX ? Vector3.left : Vector3.right;
        Vector3 startPos = transform.position + new Vector3((DebaraSprite.flipX ? -1 : 1) * offsetX, offsetY, 0);

        for (int i = 0; i < laserCount; i++)
        {
            Vector3 spawnPos = startPos + direction * spawnDistanceStep * i;
            GameObject laser = Instantiate(laserPrefab, spawnPos, Quaternion.identity);

            if (DebaraSprite.flipX)
            {

                Vector3 scale = laser.transform.localScale;
                scale.x = Mathf.Abs(scale.x) * -1f;
                laser.transform.localScale = scale;


                laser.transform.rotation = Quaternion.Euler(0, 180f, 0);
            }
            else
            {

                Vector3 scale = laser.transform.localScale;
                scale.x = Mathf.Abs(scale.x);
                laser.transform.localScale = scale;

                laser.transform.rotation = Quaternion.identity;
            }

            yield return new WaitForSeconds(interval);
        }


        StartCoroutine(ResetAttackInputCooldown());
    }
    public void FireLaser()
    {
        StartCoroutine(SpawnLaserSequence());
    }
    public void EndLaserAttack()
    {
        isAttacking = false;
    }
    public void ResetLaserSkill()
    {
        isLaserOnCooldown = false;
        isAttacking = false;
        StopAllCoroutines(); 

        if (laserCooldownUI != null)
        {
            laserCooldownUI.cooldownTime = laserCooldown; 
            laserCooldownUI.StartCooldown();
        }

        if (DebaraAnime != null && gameObject.activeInHierarchy)
        {
            DebaraAnime.ResetTrigger("Cast1");
            DebaraAnime.Play("DevaIdle");
        }

    }
    [SerializeField] private DevaBigLaserSkill bigLaserSkill;
    public void StartisInvincible()
    {
               isInvincible = true;
    }
    public void EndisInvincible()
    {
        isInvincible = false;
    }
    public void EndBigLaserFromAnimation()
    {
        if (bigLaserSkill != null)
            bigLaserSkill.EndBigLaser();
    }

}
