// My version

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerController : Singleton<PlayerController>
{
    // State names (must match your Animator state names)
    private const string STATE_IDLE = "Player_Idle";
    private const string STATE_WALK = "Player_Walk";
    private const string STATE_JUMP = "Player_Jump";
    private const string STATE_DASH = "Player_Dash";
    private const string STATE_ATTACK = "Player_Attack";
    private const string STATE_JUMP_ATTACK = "Player_Jump_Attack";
    private const string STATE_HURT = "Player_Hurt";
    private const string STATE_HEALING = "Player_Heal";
    private const string STATE_DEATH = "Player_Death";
    private const string STATE_CASTING = "Player_Cast";

    [Header("General Settings")]
    [SerializeField]
    private PlayerControls _playerControls;

    [SerializeField]
    private PlayerStateList playerState;

    [SerializeField]
    private Rigidbody2D _rigidbody2D;

    [SerializeField]
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private float xAxis,
        yAxis;
    private bool canFlash = true;
    private float _gravity;
    private string _currentState;
    private bool openMap;

    [Space(5)]
    [Header("Horizontal Movement Settings")]
    [SerializeField]
    private float walkSpeed = 1f;

    [Space(5)]
    [Header("Vertical Movement Settings")]
    [SerializeField]
    private float jumpForce = 45f;
    private float jumpBufferCounter = 0;

    [SerializeField]
    private float jumpBufferFrames;
    private float coyoteTimeCounter = 0;

    [SerializeField]
    private float coyoteTime;
    private int airJumpCounter = 0;

    [SerializeField]
    private int maxAirJumps;

    [SerializeField]
    private int maxFallingSpeed;

    [Space(5)]
    [Header("Wall Jump Settings")]
    [SerializeField]
    private float wallSlidingSpeed = 2f;

    [SerializeField]
    private Transform wallCheck;

    [SerializeField]
    private LayerMask whatIsWall;

    [SerializeField]
    private float wallJumpingDuration;

    [SerializeField]
    private Vector2 wallJumpingPower;
    private float wallJumpingDirection;
    private bool isWallSliding;
    private bool isWallJumping;

    [Space(5)]
    [Header("Ground Check Settings")]
    [SerializeField]
    private Transform groundCheckPoint;

    [SerializeField]
    private float groundCheckY = 0.2f;

    [SerializeField]
    private float groundCheckX = 0.5f;

    [SerializeField]
    private LayerMask whatIsGround;

    [Space(5)]
    [Header("Dash Settings")]
    [SerializeField]
    private float dashSpeed;

    [SerializeField]
    private float dashTime;

    [SerializeField]
    private float dashCooldown;

    [SerializeField]
    private GameObject dashEffectVFXPrefab;

    [SerializeField]
    private Transform dashEffectOrigin;
    private bool canDash = true;
    private bool dashed;

    [Space(5)]
    [Header("Attack Settings:")]
    [SerializeField]
    private Transform SideAttackTransform;

    [SerializeField]
    private Vector2 SideAttackArea;

    [SerializeField]
    private Transform UpAttackTransform;

    [SerializeField]
    private Vector2 UpAttackArea;

    [SerializeField]
    private Transform DownAttackTransform;

    [SerializeField]
    private Vector2 DownAttackArea;

    [SerializeField]
    private LayerMask attackableLayer;

    [SerializeField]
    private float timeBetweenAttacks;
    private float timeSinceAttack;
    private string _attackAnimationStarted;

    [SerializeField]
    private float damage;

    [SerializeField]
    private GameObject slashEffectWideVFXPrefab;

    [SerializeField]
    private float hitForce;
    private bool restoreTime;
    private float restoreTimeSpeed;

    [Space(5)]
    [Header("Recoil Settings:")]
    [SerializeField]
    private int recoilXSteps = 5;

    [SerializeField]
    private int recoilYSteps = 5;

    [SerializeField]
    private float recoilXSpeed = 100f;

    [SerializeField]
    private float recoilYSpeed = 100f;
    private int stepsXRecoiled,
        stepsYRecoiled;

    [Space(5)]
    [Header("Health Settings:")]
    [SerializeField]
    private int health;

    [SerializeField]
    private int maxHealth;

    [SerializeField]
    private int maxTotalHealth;

    [SerializeField]
    private int heartShards;

    [SerializeField]
    private float invincibilityDuration = 1f;

    [SerializeField]
    private GameObject bloodSpurtVFXPrefab;

    [SerializeField]
    private float hitFlashSpeed;
    private bool hitStopActive;
    public delegate void OnHealthChangedDelegate();

    [HideInInspector]
    public OnHealthChangedDelegate OnHealthChangedCallback;
    private float healTimer;

    [SerializeField]
    private float timeToHeal;
    private HealingPhase currentHealingPhase = HealingPhase.None;

    private enum HealingPhase
    {
        None,
        Starting, // 0.0 - 0.5 in blend tree
        Looping, // Stays at 0.5
        Ending, // 0.5 - 1.0 in blend tree
    }

    [Space(5)]
    [Header("Mana Settings:")]
    [SerializeField]
    private Image manaStorage;

    [SerializeField]
    private float mana;

    [SerializeField]
    private float manaDrainSpeed;

    [SerializeField]
    private float manaGain;

    [SerializeField]
    private float healBlendSpeed = 2f; // Speed to reach loop animation
    private float healBlendValue = 0f; // Current position in blend tree
    private bool halfMana;

    [SerializeField]
    private ManaOrbsHandler manaOrbsHandler;

    [SerializeField]
    private int orbShard;

    [SerializeField]
    private int manaOrbs;

    [Space(5)]
    [Header("Spell Casting Settings:")]
    [SerializeField]
    private float manaSpellCost = 0.3f;

    [SerializeField]
    private float timeBetweenCasts = 0.3f;

    [SerializeField]
    private float spellDamage; // upspell and downspell damage

    [SerializeField]
    private float downSpellForce; // Dive down force

    [SerializeField]
    private GameObject sideSpellFireball;

    [SerializeField]
    private GameObject upSpellExplosion;

    [SerializeField]
    private GameObject downSpellFireball;

    [SerializeField]
    private AnimationClip spellCastAnimation;
    private float timeSinceCast;
    private float castOrHealTimer;

    [Space(5)]
    [Header("Camera Stuff")]
    [SerializeField]
    private float playerFallSpeedThreshold = -10;

    [Space(5)]
    //Button Input values
    [HideInInspector]
    public Vector2 MoveValue;

    [HideInInspector]
    public bool JumpValue;

    [HideInInspector]
    public bool JumpValueRelease;

    [HideInInspector]
    public bool DashValue;

    [HideInInspector]
    public bool AttackValue;

    [HideInInspector]
    public bool InteractValue;

    [HideInInspector]
    public bool MapValue;

    [HideInInspector]
    public bool SaveValue;

    [HideInInspector]
    public bool OpenInventoryValue;

    [Space(5)]
    [Header("Skill Unlocks:")]
    [SerializeField]
    private bool unlockedWallJump;

    [SerializeField]
    private bool unlockedDash;

    [SerializeField]
    private bool unlockedVarJump;

    [SerializeField]
    private bool unlockedSideCast;

    [SerializeField]
    private bool unlockedUpCast;

    [SerializeField]
    private bool unlockedDownCast;

    protected override void Awake()
    {
        base.Awake();

        if (Instance != this)
            return;

        _playerControls = new PlayerControls();
        _playerControls.Player.Enable();
    }

    private void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        playerState = GetComponent<PlayerStateList>();
        _spriteRenderer = GetComponent<SpriteRenderer>();

        Health = maxHealth;
        _gravity = _rigidbody2D.gravityScale;

        Mana = mana;
        manaStorage.fillAmount = Mana;

        SaveData.Instance.LoadPlayerData();

        if (halfMana)
        {
            UIManager.Instance.SwitchManaState(UIManager.ManaState.HalfMana);
        }
        else
        {
            UIManager.Instance.SwitchManaState(UIManager.ManaState.FullMana);
        }

        if (Health == 0)
        {
            playerState.IsAlive = false;
            GameManager.Instance.RespawnPlayer();
        }
    }

    private void OnEnable()
    {
        _playerControls.Player.Enable();
    }

    protected override void OnDisable()
    {
        base.OnDisable(); // Let base class check if this is a duplicate instance

        _playerControls.Player.Disable();
    }

    private void Update()
    {
        if (playerState.IsInCutscene)
            return;

        if (!playerState.IsAlive)
        {
            UpdateAnimationState(); // Still need death animation
            return;
        }

        UpdateInput();
        UpdateJumpVariables();
        timeSinceAttack += Time.deltaTime;
        RestoreTimeScale();
        UpdateCameraYDampForPlayerFall();
        ToggleMap();
        ToggleInventory();

        if (playerState.IsDashing)
            return; // Block other actions, animation handled at end

        if (playerState.IsAttacking)
        {
            if (timeSinceAttack >= timeBetweenAttacks)
            {
                playerState.IsAttacking = false;
            }
            else
            {
                return; // Block movement while attacking
            }
        }

        if (!isWallJumping)
        {
            HandleMovement();
            HandlePlayerSpriteFlip();
            HandleJumping();
        }

        if (unlockedWallJump)
        {
            WallSlide();
            WallJump();
        }
        if (unlockedDash)
        {
            HandleDashing();
        }

        HandleAttacking();
        HandleCastingSpell();
        FlashWhileInvincible();
        HandleHealing();

        UpdateAnimationState(); // Single call - handles everything

        if (manaOrbs > 3)
        {
            manaOrbs = 3;
        }
    }

    private void FixedUpdate()
    {
        if (playerState.IsInCutscene)
            return;

        if (playerState.IsDashing || playerState.IsHealing)
            return;

        HandleRecoiling();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<EnemyCore>() != null && playerState.IsCasting)
        {
            collision
                .GetComponent<EnemyCore>()
                .EnemyGetsHit(
                    spellDamage,
                    (collision.transform.position - transform.position).normalized,
                    -recoilYSpeed
                );
        }
    }

    private void UpdateInput()
    {
        MoveValue = _playerControls.Player.Move.ReadValue<Vector2>();
        JumpValue = _playerControls.Player.Jump.WasPressedThisFrame();
        JumpValueRelease = _playerControls.Player.Jump.WasReleasedThisFrame();
        DashValue = _playerControls.Player.Dash.WasPressedThisFrame();
        AttackValue = _playerControls.Player.Attack.WasPressedThisFrame();
        InteractValue = _playerControls.Player.Interact.WasPressedThisFrame();
        MapValue = _playerControls.Player.Map.IsPressed();
        SaveValue = _playerControls.Player.Save.WasPressedThisFrame();
        openMap = MapValue;
        OpenInventoryValue = _playerControls.Player.Inventory.WasPressedThisFrame();

        xAxis = MoveValue.x;
        yAxis = MoveValue.y;

        if (_playerControls.Player.CastAndHeal.IsPressed())
        {
            castOrHealTimer += Time.deltaTime;
        }
        else
        {
            castOrHealTimer = 0;
        }
    }

    private void ToggleMap()
    {
        if (openMap)
        {
            UIManager.Instance.MapHandler.SetActive(true);
        }
        else
        {
            UIManager.Instance.MapHandler.SetActive(false);
        }
    }

    private void ToggleInventory()
    {
        if (OpenInventoryValue)
        {
            UIManager.Instance.Inventory.SetActive(true);
        }
        else
        {
            UIManager.Instance.Inventory.SetActive(false);
        }
    }

    public void Respawned()
    {
        if (!playerState.IsAlive)
        {
            _rigidbody2D.constraints = RigidbodyConstraints2D.None;
            _rigidbody2D.constraints = RigidbodyConstraints2D.FreezeRotation;
            GetComponent<Collider2D>().enabled = true;

            playerState.IsAlive = true;
            halfMana = true;
            UIManager.Instance.SwitchManaState(UIManager.ManaState.HalfMana);
            Mana = 0f;
            Health = maxHealth;
            // Force Idle state
            _currentState = STATE_IDLE;
            UpdateAnimationState();
        }
    }

    public Vector2 GetPlayerMovementDirection()
    {
        return new Vector2(xAxis, 0);
    }

    public void HandleMovement()
    {
        if (playerState.IsHealing)
        {
            _rigidbody2D.linearVelocity = Vector2.zero;
        }

        _rigidbody2D.linearVelocity = new Vector2(xAxis * walkSpeed, _rigidbody2D.linearVelocity.y);
    }

    private void HandleCastingSpell()
    {
        if (
            (_playerControls.Player.CastAndHeal.WasReleasedThisFrame())
            && (castOrHealTimer <= 0.1f)
            && (timeSinceCast >= timeBetweenCasts)
            && (Mana >= manaSpellCost)
        )
        {
            playerState.IsCasting = true;
            timeSinceCast = 0f;
        }
        else
        {
            timeSinceCast += Time.deltaTime;
        }

        if (!_playerControls.Player.CastAndHeal.IsPressed())
        {
            castOrHealTimer = 0f;
        }

        if (Grounded())
        {
            downSpellFireball.SetActive(false);
        }

        if (downSpellFireball.activeInHierarchy)
        {
            _rigidbody2D.linearVelocity = downSpellForce * Vector2.down;
        }
    }

    public bool Grounded()
    {
        if (
            Physics2D.Raycast(groundCheckPoint.position, Vector2.down, groundCheckY, whatIsGround)
            || Physics2D.Raycast(
                groundCheckPoint.position + new Vector3(groundCheckX, 0, 0),
                Vector2.down,
                groundCheckY,
                whatIsGround
            )
            || Physics2D.Raycast(
                groundCheckPoint.position + new Vector3(-groundCheckX, 0, 0),
                Vector2.down,
                groundCheckY,
                whatIsGround
            )
        )
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void HandleJumping()
    {
        if (jumpBufferCounter > 0 && coyoteTimeCounter > 0 && !playerState.IsJumping)
        {
            _rigidbody2D.linearVelocity = new Vector3(_rigidbody2D.linearVelocity.x, jumpForce, 0);
            playerState.IsJumping = true;
        }

        if (!Grounded() && airJumpCounter < maxAirJumps && JumpValue && unlockedVarJump)
        {
            playerState.IsJumping = true;
            airJumpCounter++;
            _rigidbody2D.linearVelocity = new Vector3(_rigidbody2D.linearVelocity.x, jumpForce, 0);
        }

        // if (JumpValue && _rigidbody2D.linearVelocity.y > 0 && !playerState.IsAttacking)
        if (JumpValueRelease && _rigidbody2D.linearVelocity.y > 3f)
        {
            playerState.IsJumping = false;
            _rigidbody2D.linearVelocity = new Vector2(_rigidbody2D.linearVelocity.x, 0);
        }

        _rigidbody2D.linearVelocity = new Vector2(
            _rigidbody2D.linearVelocity.x,
            Mathf.Clamp(
                _rigidbody2D.linearVelocity.y,
                -maxFallingSpeed,
                _rigidbody2D.linearVelocity.y
            )
        );
    }

    private void UpdateJumpVariables()
    {
        if (Grounded())
        {
            playerState.IsJumping = false;
            coyoteTimeCounter = coyoteTime;
            airJumpCounter = 0;
            jumpBufferCounter = 0;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        if (JumpValue)
        {
            jumpBufferCounter = jumpBufferFrames;
        }
        else if (jumpBufferCounter > 0)
        {
            jumpBufferCounter -= Time.deltaTime * 10f;
        }
    }

    private bool Walled()
    {
        return Physics2D.OverlapCircle(wallCheck.position, 0.2f, whatIsWall);
    }

    private void WallSlide()
    {
        if (Walled() && !Grounded() && xAxis != 0)
        {
            isWallSliding = true;

            _rigidbody2D.linearVelocity = new Vector2(
                _rigidbody2D.linearVelocity.x,
                Mathf.Clamp(_rigidbody2D.linearVelocity.y, -wallSlidingSpeed, float.MaxValue)
            );
        }
        else
        {
            isWallSliding = false;
        }
    }

    private void WallJump()
    {
        if (isWallSliding)
        {
            isWallJumping = false;
            wallJumpingDirection = !playerState.IsLookingRight ? 1 : -1;

            CancelInvoke(nameof(StopWallJumping));
        }

        if (JumpValue && isWallSliding)
        {
            isWallJumping = true;
            _rigidbody2D.linearVelocity = new Vector2(
                wallJumpingPower.x * wallJumpingDirection,
                wallJumpingPower.y
            );

            dashed = false;
            airJumpCounter = 0;

            if (
                (playerState.IsLookingRight && transform.eulerAngles.y == 0)
                || (!playerState.IsLookingRight && transform.eulerAngles.y != 180)
            )
            {
                playerState.IsLookingRight = !playerState.IsLookingRight;
                int yRotation = playerState.IsLookingRight ? 0 : 180;

                transform.eulerAngles = new Vector2(transform.eulerAngles.x, yRotation);
            }

            Invoke(nameof(StopWallJumping), wallJumpingDuration);
        }
    }

    private void StopWallJumping()
    {
        isWallJumping = false;
    }

    private void HandlePlayerSpriteFlip()
    {
        if (xAxis < 0)
        {
            // transform.localScale = new Vector2(-1, transform.localScale.y);
            transform.eulerAngles = new Vector2(0, 180);
            playerState.IsLookingRight = false;
        }
        else if (xAxis > 0)
        {
            // transform.localScale = new Vector2(1, transform.localScale.y);
            transform.eulerAngles = new Vector2(0, 0);
            playerState.IsLookingRight = true;
        }
    }

    private void HandleDashing()
    {
        if (canDash && _playerControls.Player.Dash.WasPressedThisFrame() && !dashed)
        {
            StartCoroutine(DashRoutine());
            dashed = true;
        }

        if (Grounded())
        {
            dashed = false;
        }
    }

    private IEnumerator DashRoutine()
    {
        canDash = false;
        playerState.IsDashing = true;
        _rigidbody2D.gravityScale = 0;
        int direction = playerState.IsLookingRight ? 1 : -1;
        _rigidbody2D.linearVelocity = new Vector2(direction * dashSpeed, 0);
        if (Grounded())
        {
            GameObject dashEffect = Instantiate(dashEffectVFXPrefab, transform);
        }
        yield return new WaitForSeconds(dashTime);
        _rigidbody2D.gravityScale = _gravity;
        playerState.IsDashing = false;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private void HandleAttacking()
    {
        if (AttackValue && timeSinceAttack >= timeBetweenAttacks && !playerState.IsInvincible)
        {
            timeSinceAttack = 0;
            playerState.IsAttacking = true;

            // Determine and lock in which attack animation to use
            _attackAnimationStarted = Grounded() ? STATE_ATTACK : STATE_JUMP_ATTACK;

            if (yAxis == 0 || yAxis < 0 && Grounded())
            {
                int recoilLeftOrRight = playerState.IsLookingRight ? 1 : -1;

                Hit(
                    SideAttackTransform,
                    SideAttackArea,
                    ref playerState.IsRecoilingXAxis,
                    Vector2.right * recoilLeftOrRight,
                    recoilXSpeed
                );

                GameObject slashEffect = Instantiate(slashEffectWideVFXPrefab, SideAttackTransform);
                // SlashEffectAtAngle(slashEffectWideVFXPrefab, 0 , SideAttackTransform);
            }
            else if (yAxis > 0)
            {
                Hit(
                    UpAttackTransform,
                    UpAttackArea,
                    ref playerState.IsRecoilingYAxis,
                    Vector2.up,
                    recoilYSpeed
                );
                SlashEffectAtAngle(slashEffectWideVFXPrefab, 80, UpAttackTransform);
            }
            else if (yAxis < 0 && !Grounded())
            {
                Hit(
                    DownAttackTransform,
                    DownAttackArea,
                    ref playerState.IsRecoilingYAxis,
                    Vector2.down,
                    recoilYSpeed
                );
                SlashEffectAtAngle(slashEffectWideVFXPrefab, -90, DownAttackTransform);
            }
        }
    }

    private void Hit(
        Transform attackTransform,
        Vector3 attackArea,
        ref bool recoilBool,
        Vector2 recoilDir,
        float recoilStrength
    )
    {
        Collider2D[] objectsToHit = Physics2D.OverlapBoxAll(
            attackTransform.position,
            attackArea,
            0,
            attackableLayer
        );
        List<EnemyCore> enemiesHit = new List<EnemyCore>();

        if (objectsToHit.Length > 0)
        {
            recoilBool = true;
        }

        for (int i = 0; i < objectsToHit.Length; i++)
        {
            if (objectsToHit[i].GetComponent<EnemyCore>() != null)
            {
                EnemyCore enemy = objectsToHit[i].GetComponent<EnemyCore>();
                if (enemy && !enemiesHit.Contains(enemy))
                {
                    enemy.EnemyGetsHit(damage, recoilDir, recoilStrength);
                    enemiesHit.Add(enemy);
                }

                if (objectsToHit[i].GetComponent<EnemyCore>())
                {
                    Mana += manaGain;
                }
            }
        }
    }

    private void SlashEffectAtAngle(
        GameObject slashEffect,
        int effectAngle,
        Transform attackTransform
    )
    {
        slashEffect = Instantiate(slashEffect, attackTransform);
        slashEffect.transform.eulerAngles = new Vector3(0, 0, effectAngle);
        slashEffect.transform.localScale = new Vector2(
            transform.localScale.x,
            transform.localScale.y
        );
    }

    private void HandleRecoiling()
    {
        if (playerState.IsRecoilingXAxis)
        {
            if (playerState.IsLookingRight)
            {
                _rigidbody2D.linearVelocity = new Vector2(-recoilXSpeed, 0);
            }
            else
            {
                _rigidbody2D.linearVelocity = new Vector2(recoilXSpeed, 0);
            }
        }

        if (playerState.IsRecoilingYAxis)
        {
            _rigidbody2D.gravityScale = 0;
            if (yAxis < 0)
            {
                _rigidbody2D.linearVelocity = new Vector2(
                    _rigidbody2D.linearVelocity.x,
                    recoilYSpeed
                );
            }
            else
            {
                _rigidbody2D.linearVelocity = new Vector2(
                    _rigidbody2D.linearVelocity.x,
                    -recoilYSpeed
                );
            }

            airJumpCounter = 0;
        }
        else
        {
            _rigidbody2D.gravityScale = _gravity;
        }

        //Stop Recoil
        if (playerState.IsRecoilingXAxis && stepsXRecoiled < recoilXSteps)
        {
            stepsXRecoiled++;
        }
        else
        {
            StopRecoilX();
        }

        if (playerState.IsRecoilingYAxis && stepsYRecoiled < recoilYSteps)
        {
            stepsYRecoiled++;
        }
        else
        {
            StopRecoilY();
        }

        if (Grounded())
        {
            StopRecoilY();
        }
    }

    private void StopRecoilX()
    {
        stepsXRecoiled = 0;
        playerState.IsRecoilingXAxis = false;
    }

    private void StopRecoilY()
    {
        stepsYRecoiled = 0;
        playerState.IsRecoilingYAxis = false;
    }

    public void TakeDamage(float damage)
    {
        if (playerState.IsAlive)
        {
            Health -= Mathf.RoundToInt(damage);

            // Cancel any ongoing attacks
            playerState.IsAttacking = false;
            _attackAnimationStarted = null;

            if (Health <= 0)
            {
                playerState.IsAlive = false;
                Health = 0;
                StartCoroutine(DeathRoutine());
                return;
            }
            else
            {
                StartCoroutine(StopTakingDamageRoutine());
            }
        }
    }

    private IEnumerator StopTakingDamageRoutine()
    {
        playerState.IsInvincible = true;
        var fx = Instantiate(bloodSpurtVFXPrefab, transform.position, Quaternion.identity);

        // use realtime so invincibility always ends even if timeScale changes
        yield return new WaitForSecondsRealtime(invincibilityDuration);
        playerState.IsInvincible = false;
    }

    private void FlashWhileInvincible()
    {
        if (playerState.IsInvincible && !playerState.IsInCutscene)
        {
            if (Time.timeScale > 0.2 && canFlash)
            {
                StartCoroutine(FlashRoutine());
            }
        }
        else
        {
            _spriteRenderer.enabled = true;
        }
    }

    private IEnumerator FlashRoutine()
    {
        _spriteRenderer.enabled = !_spriteRenderer.enabled;
        canFlash = false;
        yield return new WaitForSeconds(0.1f);
        canFlash = true;
    }

    private void RestoreTimeScale()
    {
        if (restoreTime)
        {
            if (Time.timeScale < 1)
            {
                Time.timeScale += Time.unscaledDeltaTime * restoreTimeSpeed;
            }
            else
            {
                Time.timeScale = 1;
                restoreTime = false;
                hitStopActive = false;
            }
        }
    }

    public void HitStopTime(float newTimeScale, int restoreSpeed, float delay)
    {
        if (hitStopActive)
            return;
        hitStopActive = true;

        restoreTimeSpeed = restoreSpeed;
        Time.timeScale = Mathf.Clamp(newTimeScale, 0f, 1f);

        if (delay > 0f)
        {
            // don’t rely on scaled time when timeScale might be 0
            StopCoroutine(nameof(StartTimeAgain));
            StartCoroutine(StartTimeAgain(delay));
        }
        else
        {
            restoreTime = true;
        }
    }

    private IEnumerator StartTimeAgain(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        restoreTime = true; // will ramp time back using unscaled delta
    }

    public int Health
    {
        get { return health; }
        set
        {
            if (health != value)
            {
                health = Mathf.Clamp(value, 0, maxHealth);

                if (OnHealthChangedCallback != null)
                {
                    OnHealthChangedCallback.Invoke();
                }
            }
        }
    }

    private void HandleHealing()
    {
        bool wantsToHeal =
            _playerControls.Player.CastAndHeal.IsPressed()
            && castOrHealTimer > 0.1f
            && Health < maxHealth
            && Mana > 0
            && Grounded()
            && !playerState.IsDashing;

        // Starting to heal
        if (wantsToHeal && currentHealingPhase == HealingPhase.None)
        {
            playerState.IsHealing = true;
            currentHealingPhase = HealingPhase.Starting;
            healBlendValue = 0f;
        }

        // Currently healing
        if (playerState.IsHealing)
        {
            // STARTING PHASE: Blend from start (0) to loop (0.5)
            if (currentHealingPhase == HealingPhase.Starting)
            {
                healBlendValue += Time.deltaTime * healBlendSpeed;

                if (healBlendValue >= 0.5f)
                {
                    healBlendValue = 0.5f;
                    currentHealingPhase = HealingPhase.Looping;
                }

                _animator.SetFloat("Motion", healBlendValue);
            }
            // LOOPING PHASE: Stay at 0.5, perform healing
            else if (currentHealingPhase == HealingPhase.Looping)
            {
                healBlendValue = 0.5f; // Keep at loop
                _animator.SetFloat("Motion", healBlendValue);

                if (wantsToHeal)
                {
                    // Perform healing
                    healTimer += Time.deltaTime;
                    if (healTimer >= timeToHeal)
                    {
                        Health += 1;
                        healTimer = 0;
                    }

                    // Drain Mana
                    manaOrbsHandler.usedMana = true;
                    manaOrbsHandler.countDown = 3f;
                    Mana -= Time.deltaTime * manaDrainSpeed;

                    // Check if healing should stop
                    if (!playerState.IsHealing || Health >= maxHealth || Mana <= 0)
                    {
                        // Begin ending animation
                        currentHealingPhase = HealingPhase.Ending;
                    }
                }
                else
                {
                    // Button released or can't heal anymore - start ending
                    currentHealingPhase = HealingPhase.Ending;
                    healBlendValue = 0.5f; // Start from loop position
                }
            }
            // ENDING PHASE: Blend from loop (0.5) to end (1.0)
            else if (currentHealingPhase == HealingPhase.Ending)
            {
                healBlendValue += Time.deltaTime * healBlendSpeed;

                if (healBlendValue >= 1f)
                {
                    healBlendValue = 1f;
                    // End animation complete - reset everything
                    playerState.IsHealing = false;
                    currentHealingPhase = HealingPhase.None;
                    healTimer = 0;
                    healBlendValue = 0f;
                }

                _animator.SetFloat("Motion", healBlendValue);
            }
        }
    }

    public float Mana
    {
        get { return mana; }
        set
        {
            if (mana != value)
            {
                if (!halfMana)
                {
                    mana = Mathf.Clamp(value, 0, 1);
                }
                else
                {
                    mana = Mathf.Clamp(value, 0, 0.5f);
                }
                manaStorage.fillAmount = Mana;
            }
        }
    }

    public void RestoreMana()
    {
        halfMana = false;
        UIManager.Instance.SwitchManaState(UIManager.ManaState.FullMana);
    }

    public IEnumerator WalkIntoNewSceneRoutine(Vector2 exitDir, float delay)
    {
        playerState.IsInvincible = true;

        if (exitDir.y > 0)
        {
            _rigidbody2D.linearVelocity = new Vector2(0, walkSpeed);
        }

        if (exitDir.x != 0)
        {
            xAxis = exitDir.x > 0 ? 1 : -1;

            HandleMovement();
        }
        HandlePlayerSpriteFlip();

        yield return new WaitForSecondsRealtime(delay);
        playerState.IsInvincible = false;
        playerState.IsInCutscene = false;
    }

    private IEnumerator DeathRoutine()
    {
        playerState.IsAlive = false;
        Time.timeScale = 1f;
        GameObject deathEffect = Instantiate(
            bloodSpurtVFXPrefab,
            transform.position,
            Quaternion.identity
        );
        Destroy(deathEffect, 1.5f);

        _rigidbody2D.constraints = RigidbodyConstraints2D.FreezePosition;
        GetComponent<Collider2D>().enabled = false;

        yield return new WaitForSeconds(0.9f);
        StartCoroutine(UIManager.Instance.ActivateDeathScreenRoutine());

        yield return new WaitForSeconds(0.8f);
        GameObject shadeGO = Instantiate(
            GameManager.Instance.PlayerShade,
            transform.position,
            Quaternion.identity
        );
    }

    private void UpdateAnimationState()
    {
        string newState;

        if (Health <= 0 && _currentState != STATE_DEATH)
        {
            Health = 0;
            newState = STATE_DEATH;
        }
        // HURT should have high priority
        else if (playerState.IsInvincible && health > 0)
        {
            newState = STATE_HURT;
        }
        // IMPORTANT: Keep healing state active during ALL phases (including ending)
        else if (playerState.IsHealing || currentHealingPhase != HealingPhase.None)
        {
            newState = STATE_HEALING;
            // The Motion parameter is already being updated in HandleHealing()
        }
        else if (playerState.IsCasting)
        {
            newState = STATE_CASTING;
        }
        else if (playerState.IsAttacking)
        {
            newState = _attackAnimationStarted;
        }
        else if (playerState.IsDashing)
        {
            newState = STATE_DASH;
        }
        else if (!Grounded())
        {
            newState = STATE_JUMP;
        }
        else if (Mathf.Abs(_rigidbody2D.linearVelocity.x) > 0.01f)
        {
            newState = STATE_WALK;
        }
        else if (_currentState == STATE_IDLE)
        {
            newState = STATE_IDLE;
        }
        else
        {
            newState = STATE_IDLE;
        }

        if (newState != _currentState)
        {
            _animator.Play(newState);
            _currentState = newState;
        }
    }

    public void OnSpellCastFrame()
    {
        // This will be called by the Animation Event
        //Side cast
        if ((yAxis == 0 || (yAxis < 0 && Grounded())) && unlockedSideCast)
        {
            GameObject spell = Instantiate(
                sideSpellFireball,
                SideAttackTransform.position,
                Quaternion.identity
            );

            if (playerState.IsLookingRight)
            {
                spell.transform.rotation = Quaternion.Euler(0, 0, 0);
            }
            else
            {
                spell.transform.rotation = Quaternion.Euler(0, 180, 0);
            }
            playerState.IsRecoilingXAxis = true;
        }
        //Up cast
        else if (yAxis > 0 && unlockedUpCast)
        {
            GameObject spell = Instantiate(upSpellExplosion, transform);
            _rigidbody2D.linearVelocity = Vector2.zero;
        }
        //Down cast
        else if (yAxis < 0 && unlockedDownCast)
        {
            downSpellFireball.SetActive(true);
        }

        Mana -= manaSpellCost;
    }

    public void OnSpellCastEnd()
    {
        playerState.IsCasting = false;
    }

    private void UpdateCameraYDampForPlayerFall()
    {
        //if falling past a certain speed threshold
        if (
            _rigidbody2D.linearVelocity.y < playerFallSpeedThreshold
            && !CameraManager.Instance.IsLerpingYDamping
            && !CameraManager.Instance.HasLerpedYDamping
        )
        {
            StartCoroutine(CameraManager.Instance.LerpYDamping(true));
        }
        //if standing stil or moving up
        if (
            _rigidbody2D.linearVelocity.y >= 0
            && !CameraManager.Instance.IsLerpingYDamping
            && CameraManager.Instance.HasLerpedYDamping
        )
        {
            //reset camera function
            CameraManager.Instance.HasLerpedYDamping = false;
            StartCoroutine(CameraManager.Instance.LerpYDamping(false));
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(SideAttackTransform.position, SideAttackArea);
        Gizmos.DrawWireCube(UpAttackTransform.position, UpAttackArea);
        Gizmos.DrawWireCube(DownAttackTransform.position, DownAttackArea);
    }

    public PlayerStateList PlayerStateList
    {
        get { return playerState; }
        set { playerState = value; }
    }

    public int GetHealth() => health;

    public int GetMaxHealth() => maxHealth;

    public Rigidbody2D _Rigidbody2D
    {
        get { return _rigidbody2D; }
        set { _rigidbody2D = value; }
    }

    public bool HalfMana
    {
        get { return halfMana; }
        set { halfMana = value; }
    }

    public int MaxHealth
    {
        get { return maxHealth; }
        set { maxHealth = value; }
    }

    public PlayerControls GetPlayerControls()
    {
        return _playerControls;
    }

    public bool UnlockedWallJump
    {
        get => unlockedWallJump;
        set => unlockedWallJump = value;
    }

    public bool UnlockedDash
    {
        get => unlockedDash;
        set => unlockedDash = value;
    }

    public bool UnlockedVarJump
    {
        get => unlockedVarJump;
        set => unlockedVarJump = value;
    }

    public bool UnlockedDownCast
    {
        get => unlockedDownCast;
        set => unlockedDownCast = value;
    }

    public bool UnlockedSideCast
    {
        get => unlockedSideCast;
        set => unlockedSideCast = value;
    }

    public bool UnlockedUpCast
    {
        get => unlockedUpCast;
        set => unlockedUpCast = value;
    }

    public int MaxTotalHealth
    {
        get => maxTotalHealth;
        set => maxTotalHealth = value;
    }

    public int HeartShards
    {
        get => heartShards;
        set => heartShards = value;
    }

    public int OrbShard
    {
        get => orbShard;
        set => orbShard = value;
    }

    public int ManaOrbs
    {
        get => manaOrbs;
        set => manaOrbs = value;
    }

    public ManaOrbsHandler ManaOrbsHandler
    {
        get => manaOrbsHandler;
        set => manaOrbsHandler = value;
    }
}
