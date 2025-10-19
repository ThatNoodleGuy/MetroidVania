using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;

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
    [SerializeField] private Transform visualRoot;
    private float visualBaseXScale;
    [SerializeField] private PlayerControls _playerControls;
    [SerializeField] private PlayerStateList _playerStateList;
    [SerializeField] private Rigidbody2D _rigidbody2D;
    [SerializeField] private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private float xAxis, yAxis;
    private bool canFlash = true;
    private float _gravity;
    private string _currentState;
    [Space(5)]

    [Header("Horizontal Movement Settings")]
    [SerializeField] private float walkSpeed = 1f;
    [Space(5)]

    [Header("Vertical Movement Settings")]
    [SerializeField] private float jumpForce = 45f;
    private float jumpBufferCounter = 0;
    [SerializeField] private float jumpBufferFrames;
    private float coyoteTimeCounter = 0;
    [SerializeField] private float coyoteTime;
    private int airJumpCounter = 0;
    [SerializeField] private int maxAirJumps;
    [Space(5)]

    [Header("Ground Check Settings")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckY = 0.2f;
    [SerializeField] private float groundCheckX = 0.5f;
    [SerializeField] private LayerMask whatIsGround;
    [Space(5)]

    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed;
    [SerializeField] private float dashTime;
    [SerializeField] private float dashCooldown;
    // [SerializeField]private Transform dashEffectOriginPoint;
    [SerializeField] private GameObject dashEffectVFXPrefab;
    [SerializeField] private Transform dashEffectOrigin;   // empty child under VisualRoot at the feet
    private bool canDash = true;
    private bool dashed;
    [Space(5)]

    [Header("Attack Settings:")]
    [SerializeField] private Transform SideAttackTransform;
    [SerializeField] private Vector2 SideAttackArea;
    [SerializeField] private Transform UpAttackTransform;
    [SerializeField] private Vector2 UpAttackArea;
    [SerializeField] private Transform DownAttackTransform;
    [SerializeField] private Vector2 DownAttackArea;
    [SerializeField] private LayerMask attackableLayer;
    [SerializeField] private float timeBetweenAttacks;
    private float timeSinceAttack;
    private string _attackAnimationStarted;
    [SerializeField] private float damage;
    [SerializeField] private GameObject slashEffectWideVFXPrefab;
    [SerializeField] private float hitForce;
    private bool restoreTime;
    private float restoreTimeSpeed;
    [Space(5)]

    [Header("Recoil Settings:")]
    [SerializeField] private int recoilXSteps = 5;
    [SerializeField] private int recoilYSteps = 5;
    [SerializeField] private float recoilXSpeed = 100f;
    [SerializeField] private float recoilYSpeed = 100f;
    private int stepsXRecoiled, stepsYRecoiled;
    [Space(5)]

    [Header("Health Settings:")]
    [SerializeField] private int health;
    [SerializeField] private int maxHealth;
    [SerializeField] private float invincibilityDuration = 1f;
    [SerializeField] private GameObject bloodSpurtVFXPrefab;
    [SerializeField] private float hitFlashSpeed;
    private bool hitStopActive;
    public delegate void OnHealthChangedDelegate();
    [HideInInspector] public OnHealthChangedDelegate OnHealthChangedCallback;
    private float healTimer;
    [SerializeField] private float timeToHeal;
    [Space(5)]

    [Header("Mana Settings:")]
    [SerializeField] private Image manaStorage;
    [SerializeField] private float mana;
    [SerializeField] private float manaDrainSpeed;
    [SerializeField] private float manaGain;
    [SerializeField] private float healBlendSpeed = 2f; // Speed to reach loop animation
    private float healBlendValue = 0f; // Current position in blend tree
    [Space(5)]

    [Header("Spell Casting Settings:")]
    [SerializeField] private float manaSpellCost = 0.3f;
    [SerializeField] private float timeBetweenCasts = 0.3f;
    [SerializeField] private float spellDamage; // upspell and downspell damage
    [SerializeField] private float downSpellForce; // Dive down force
    [SerializeField] private GameObject sideSpellFireball;
    [SerializeField] private GameObject upSpellExplosion;
    [SerializeField] private GameObject downSpellFireball;
    [SerializeField] private AnimationClip spellCastAnimation;
    private float timeSinceCast;
    private float castOrHealTimer;
    [Space(5)]

    //Buttons
    private Vector2 MoveValue;
    private bool JumpValue;
    private bool JumpValueRelease;
    private bool DashValue;
    private bool AttackValue;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this && Instance != null)
        {
            Destroy(gameObject);
        }

        _playerControls = new PlayerControls();
        _playerControls.Player.Enable();

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _playerStateList = GetComponent<PlayerStateList>();
        _spriteRenderer = GetComponent<SpriteRenderer>();

        Health = maxHealth;
        _gravity = _rigidbody2D.gravityScale;

        Mana = mana;
        manaStorage.fillAmount = Mana;
    }

    private void OnEnable()
    {
        _playerControls.Player.Enable();
        _playerControls.Player.Move.Enable();
        _playerControls.Player.Jump.Enable();
        _playerControls.Player.Dash.Enable();
        _playerControls.Player.Attack.Enable();

    }

    private void OnDisable()
    {
        _playerControls.Player.Disable();
        _playerControls.Player.Move.Disable();
        _playerControls.Player.Jump.Disable();
        _playerControls.Player.Dash.Disable();
        _playerControls.Player.Attack.Disable();

    }

    private void Update()
    {
        if (_playerStateList.IsInCutscene) return;

        UpdateAxisInput();
        UpdateJumpVariables();

        timeSinceAttack += Time.deltaTime;

        if (_playerStateList.IsDashing || _playerStateList.IsHealing)
        {
            UpdateAnimationState();
            return;
        }

        if (_playerStateList.IsAttacking && timeSinceAttack >= timeBetweenAttacks)
        {
            _playerStateList.IsAttacking = false;
        }

        if (_playerStateList.IsAttacking)
        {
            // Update attack timer but don't allow movement
            UpdateAnimationState();
            return;
        }

        RestoreTimeScale();
        FlashWhileInvincible();
        HandleMovement();
        HandleHealing();
        HandleCastingSpell();

        HandlePlayerSpriteFlip();
        HandleJumping();
        HandleDashing();
        HandleAttacking();
        UpdateAnimationState();
    }

    private void FixedUpdate()
    {
        if (_playerStateList.IsInCutscene) return;

        if (_playerStateList.IsDashing || _playerStateList.IsHealing) return;

        HandleRecoiling();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<EnemyCore>() != null && _playerStateList.IsCasting)
        {
            collision.GetComponent<EnemyCore>().EnemyHit(spellDamage, (collision.transform.position - transform.position).normalized, -recoilYSpeed);
        }
    }

    private void UpdateAxisInput()
    {
        MoveValue = _playerControls.Player.Move.ReadValue<Vector2>();
        JumpValue = _playerControls.Player.Jump.WasPressedThisFrame();
        JumpValueRelease = _playerControls.Player.Jump.WasReleasedThisFrame();
        DashValue = _playerControls.Player.Dash.WasPressedThisFrame();
        AttackValue = _playerControls.Player.Attack.WasPressedThisFrame();

        xAxis = MoveValue.x;
        yAxis = MoveValue.y;

        if (_playerControls.Player.CastAndHeal.IsPressed())
        {
            castOrHealTimer += Time.deltaTime;
        }
        else
        {
            castOrHealTimer = 0f;
        }
    }

    public Vector2 GetPlayerMovementDirection()
    {
        return new Vector2(xAxis, 0);
    }

    public void HandleMovement()
    {
        if (_playerStateList.IsHealing)
        {
            _rigidbody2D.linearVelocity = Vector2.zero;
        }

        _rigidbody2D.linearVelocity = new Vector2(xAxis * walkSpeed, _rigidbody2D.linearVelocity.y);
    }

    private void HandleCastingSpell()
    {
        if ((_playerControls.Player.CastAndHeal.WasReleasedThisFrame()) && (castOrHealTimer <= 0.05f) && (timeSinceCast >= timeBetweenCasts) && (Mana >= manaSpellCost))
        {
            _playerStateList.IsCasting = true;
            timeSinceCast = 0f;
            // No coroutine needed!
        }
        else
        {
            timeSinceCast += Time.deltaTime;
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
        if (Physics2D.Raycast(groundCheckPoint.position, Vector2.down, groundCheckY, whatIsGround) ||
        Physics2D.Raycast(groundCheckPoint.position + new Vector3(groundCheckX, 0, 0), Vector2.down, groundCheckY, whatIsGround) ||
        Physics2D.Raycast(groundCheckPoint.position + new Vector3(-groundCheckX, 0, 0), Vector2.down, groundCheckY, whatIsGround))
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
        if (jumpBufferCounter > 0 && coyoteTimeCounter > 0 && !_playerStateList.IsJumping)
        {
            _rigidbody2D.linearVelocity = new Vector3(_rigidbody2D.linearVelocity.x, jumpForce, 0);
            _playerStateList.IsJumping = true;
        }
        
        if (!Grounded() && airJumpCounter < maxAirJumps && JumpValue)
        {
            _playerStateList.IsJumping = true;
            airJumpCounter++;
            _rigidbody2D.linearVelocity = new Vector3(_rigidbody2D.linearVelocity.x, jumpForce, 0);
        }

        // if (JumpValue && _rigidbody2D.linearVelocity.y > 0 && !_playerStateList.IsAttacking)
        if (JumpValueRelease && _rigidbody2D.linearVelocity.y > 3f)
        {
            _playerStateList.IsJumping = false;
            _rigidbody2D.linearVelocity = new Vector2(_rigidbody2D.linearVelocity.x, 0);
        }
    }

    private void UpdateJumpVariables()
    {
        if (Grounded())
        {
            _playerStateList.IsJumping = false;
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

    private void HandlePlayerSpriteFlip()
    {
        if (xAxis < 0)
        {
            transform.localScale = new Vector2(-1, transform.localScale.y);
            _playerStateList.IsLookingRight = false;
        }
        else if (xAxis > 0)
        {
            transform.localScale = new Vector2(1, transform.localScale.y);
            _playerStateList.IsLookingRight = true;
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
        _playerStateList.IsDashing = true;
        _rigidbody2D.gravityScale = 0;
        int direction = _playerStateList.IsLookingRight ? 1 : -1;
        _rigidbody2D.linearVelocity = new Vector2(direction * dashSpeed, 0);
        if (Grounded())
        {
            GameObject dashEffect = Instantiate(dashEffectVFXPrefab, transform);
        }
        yield return new WaitForSeconds(dashTime);
        _rigidbody2D.gravityScale = _gravity;
        _playerStateList.IsDashing = false;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private void HandleAttacking()
    {
        if (AttackValue && timeSinceAttack >= timeBetweenAttacks && !_playerStateList.IsInvincible)
        {
            timeSinceAttack = 0;
            _playerStateList.IsAttacking = true;

            // Determine and lock in which attack animation to use
            _attackAnimationStarted = Grounded() ? STATE_ATTACK : STATE_JUMP_ATTACK;

            if (yAxis == 0 || yAxis < 0 && Grounded())
            {
                Hit(SideAttackTransform, SideAttackArea, ref _playerStateList.IsRecoilingXAxis, recoilXSpeed);
                GameObject slashEffect = Instantiate(slashEffectWideVFXPrefab, SideAttackTransform);
                // SlashEffectAtAngle(slashEffectWideVFXPrefab, 0 , SideAttackTransform);
            }
            else if (yAxis > 0)
            {
                Hit(UpAttackTransform, UpAttackArea, ref _playerStateList.IsRecoilingYAxis, recoilYSpeed);
                SlashEffectAtAngle(slashEffectWideVFXPrefab, 80, UpAttackTransform);
            }
            else if (yAxis < 0 && !Grounded())
            {
                Hit(DownAttackTransform, DownAttackArea, ref _playerStateList.IsRecoilingYAxis, recoilYSpeed);
                SlashEffectAtAngle(slashEffectWideVFXPrefab, -90, DownAttackTransform);
            }
        }
    }

    private void Hit(Transform attackTransform, Vector3 attackArea, ref bool recoilDir, float recoilStrength)
    {
        Collider2D[] objectsToHit = Physics2D.OverlapBoxAll(attackTransform.position, attackArea, 0, attackableLayer);
        List<EnemyCore> enemiesHit = new List<EnemyCore>();

        if (objectsToHit.Length > 0)
        {
            recoilDir = true;
        }

        for (int i = 0; i < objectsToHit.Length; i++)
        {
            if (objectsToHit[i].GetComponent<EnemyCore>() != null)
            {
                EnemyCore enemy = objectsToHit[i].GetComponent<EnemyCore>();
                if (enemy && !enemiesHit.Contains(enemy))
                {
                    enemy.EnemyHit(damage, (transform.position - objectsToHit[i].transform.position).normalized, recoilStrength);
                    enemiesHit.Add(enemy);
                }

                if (objectsToHit[i].GetComponent<EnemyCore>())
                {
                    Mana += manaGain;
                }
            }
        }
    }

    private void SlashEffectAtAngle(GameObject slashEffect, int effectAngle, Transform attackTransform)
    {
        slashEffect = Instantiate(slashEffect, attackTransform);
        slashEffect.transform.eulerAngles = new Vector3(0, 0, effectAngle);
        slashEffect.transform.localScale = new Vector2(transform.localScale.x, transform.localScale.y);
    }

    private void HandleRecoiling()
    {
        if (_playerStateList.IsRecoilingXAxis)
        {
            if (_playerStateList.IsLookingRight)
            {
                _rigidbody2D.linearVelocity = new Vector2(-recoilXSpeed, 0);
            }
            else
            {
                _rigidbody2D.linearVelocity = new Vector2(recoilXSpeed, 0);
            }
        }

        if (_playerStateList.IsRecoilingYAxis)
        {
            _rigidbody2D.gravityScale = 0;
            if (yAxis < 0)
            {
                _rigidbody2D.linearVelocity = new Vector2(_rigidbody2D.linearVelocity.x, recoilYSpeed);
            }
            else
            {
                _rigidbody2D.linearVelocity = new Vector2(_rigidbody2D.linearVelocity.x, -recoilYSpeed);
            }

            airJumpCounter = 0;
        }
        else
        {
            _rigidbody2D.gravityScale = _gravity;
        }

        //Stop Recoil
        if (_playerStateList.IsRecoilingXAxis && stepsXRecoiled < recoilXSteps)
        {
            stepsXRecoiled++;
        }
        else
        {
            StopRecoilX();
        }

        if (_playerStateList.IsRecoilingYAxis && stepsYRecoiled < recoilYSteps)
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
        _playerStateList.IsRecoilingXAxis = false;
    }

    private void StopRecoilY()
    {
        stepsYRecoiled = 0;
        _playerStateList.IsRecoilingYAxis = false;
    }

    public void TakeDamage(float damage)
    {
        Health -= Mathf.RoundToInt(damage);
        
        // Cancel any ongoing attacks
        _playerStateList.IsAttacking = false;
        _attackAnimationStarted = null;
        
        StartCoroutine(StopTakingDamageRoutine());
    }

    private IEnumerator StopTakingDamageRoutine()
    {
        _playerStateList.IsInvincible = true;
        var fx = Instantiate(bloodSpurtVFXPrefab, transform.position, Quaternion.identity);

        // use realtime so invincibility always ends even if timeScale changes
        yield return new WaitForSecondsRealtime(invincibilityDuration);
        _playerStateList.IsInvincible = false;
    }

    private void FlashWhileInvincible()
    {
        if (_playerStateList.IsInvincible)
        {
            if(Time.timeScale > 0.2 && canFlash)
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
        if (hitStopActive) return;
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
        if ((_playerControls.Player.CastAndHeal.IsPressed()) && (castOrHealTimer > 0.05f) && (Health < maxHealth) && (Mana > 0) && (Grounded()) && (!_playerStateList.IsDashing))
        {
            _playerStateList.IsHealing = true;

            // Smoothly progress through blend tree: Start (0) -> Loop (0.5)
            healBlendValue += Time.deltaTime * healBlendSpeed;
            
            // Clamp to stay in the loop phase (between start and end)
            healBlendValue = Mathf.Clamp(healBlendValue, 0f, 0.5f);
            
            // IMPORTANT: Update this every frame while healing
            _animator.SetFloat("Motion", healBlendValue);

            // Healing logic
            healTimer += Time.deltaTime;
            if (healTimer >= timeToHeal)
            {
                Health += 1;
                healTimer = 0;
            }

            // Drain Mana
            Mana -= Time.deltaTime * manaDrainSpeed;
        }
        else
        {
            // If we were healing, play the end animation
            if (_playerStateList.IsHealing)
            {
                // Trigger end animation
                _animator.SetFloat("Motion", 1f);
            }
            
            _playerStateList.IsHealing = false;
            healTimer = 0;
            healBlendValue = 0f; // Reset immediately for next time
        }
    }

    public float Mana
    {
        get
        {
            return mana;
        }
        set
        {
            if (mana != value)
            {
                mana = Mathf.Clamp(value, 0, 1);
                manaStorage.fillAmount = Mana;
            }
        }
    }
    
    public IEnumerator WalkIntoNewSceneRoutine(Vector2 exitDir, float delay)
    {
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
        _playerStateList.IsInCutscene = false;
    }

    private void UpdateAnimationState()
    {
        string newState;

        // HURT should have high priority
        if (_playerStateList.IsInvincible && health > 0)
        {
            newState = STATE_HURT;
        }
        else if (_playerStateList.IsHealing)
        {
            newState = STATE_HEALING;
            // Keep updating the blend parameter while in healing state
            _animator.SetFloat("Motion", healBlendValue);
        }
        else if (_playerStateList.IsCasting)
        {
            newState = STATE_CASTING;
        }
        else if (_playerStateList.IsAttacking)
        {
            newState = _attackAnimationStarted;
        }
        else if (_playerStateList.IsDashing)
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
        if (yAxis == 0 || (yAxis < 0 && Grounded()))
        {
            GameObject spell = Instantiate(sideSpellFireball, SideAttackTransform.position, Quaternion.identity);

            if (_playerStateList.IsLookingRight)
            {
                spell.transform.rotation = Quaternion.Euler(0, 0, 0);
            }
            else
            {
                spell.transform.rotation = Quaternion.Euler(0, 180, 0);
            }
            _playerStateList.IsRecoilingXAxis = true;
        }
        //Up cast
        else if (yAxis > 0)
        {
            GameObject spell = Instantiate(upSpellExplosion, transform);
            _rigidbody2D.linearVelocity = Vector2.zero;
        }
        //Down cast
        else if (yAxis < 0)
        {
            downSpellFireball.SetActive(true);
        }

        Mana -= manaSpellCost;
    }

    public void OnSpellCastEnd()
    {
        _playerStateList.IsCasting = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(SideAttackTransform.position, SideAttackArea);
        Gizmos.DrawWireCube(UpAttackTransform.position, UpAttackArea);
        Gizmos.DrawWireCube(DownAttackTransform.position, DownAttackArea);
    }

    public int GetHealth() => health;
    public int GetMaxHealth() => maxHealth;
}