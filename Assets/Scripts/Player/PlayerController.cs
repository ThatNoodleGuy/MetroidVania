using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

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
    [SerializeField] private GameObject dashEffectVFXPrefab;
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
    [Space(5)]

    private PlayerControls _playerControls;
    private PlayerStateList _playerStateList;
    private Rigidbody2D _rigidbody2D;
    private Animator _animator;
    private float xAxis, yAxis;
    private float playerSpriteXScale;
    private float _gravity;
    private string _currentState;

    public Vector2 MoveValue => _playerControls.Player.Move.ReadValue<Vector2>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this && Instance != null)
        {
            Destroy(this);
        }

        _playerControls = new PlayerControls();
        _playerControls.Player.Enable();
    }

    private void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _playerStateList = GetComponent<PlayerStateList>();

        _playerControls.Player.Move.performed += _ => HandleMovement();
        _playerControls.Player.Jump.performed += _ => HandleJumping();

        health = maxHealth;

        playerSpriteXScale = transform.localScale.x;
        _gravity = _rigidbody2D.gravityScale;
    }

    private void OnEnable()
    {
        _playerControls.Player.Enable();
    }

    private void OnDisable()
    {
        _playerControls.Player.Disable();
    }

    private void Update()
    {
        UpdateAxisInput();
        UpdateJumpVariables();

        timeSinceAttack += Time.deltaTime;

        // Don't allow other actions during dash or attack
        if (_playerStateList.IsDashing)
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

        HandlePlayerSpriteFlip();
        HandleMovement();
        HandleJumping();
        HandleDashing();
        HandleAttacking();
        UpdateAnimationState();
    }

    private void UpdateAxisInput()
    {
        xAxis = MoveValue.x;
        yAxis = MoveValue.y;
    }

    public Vector2 GetPlayerMovementDirection()
    {
        return new Vector2(xAxis, 0);
    }

    public void HandleMovement()
    {
        _rigidbody2D.linearVelocity = new Vector2(xAxis * walkSpeed, _rigidbody2D.linearVelocity.y);
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
        // Don't cancel jump if attacking in the air
        if (_playerControls.Player.Jump.WasReleasedThisFrame() && _rigidbody2D.linearVelocity.y > 0 && !_playerStateList.IsAttacking)
        {
            _rigidbody2D.linearVelocity = new Vector2(_rigidbody2D.linearVelocity.x, 0);
            _playerStateList.IsJumping = false;
        }

        if (!_playerStateList.IsJumping)
        {
            if (jumpBufferCounter > 0 && coyoteTimeCounter > 0)
            {
                _rigidbody2D.linearVelocity = new Vector3(_rigidbody2D.linearVelocity.x, jumpForce, 0);
                _playerStateList.IsJumping = true;
            }
            else if (!Grounded() && airJumpCounter < maxAirJumps && _playerControls.Player.Jump.WasPressedThisFrame())
            {
                _playerStateList.IsJumping = true;
                airJumpCounter++;
                _rigidbody2D.linearVelocity = new Vector3(_rigidbody2D.linearVelocity.x, jumpForce, 0);
            }
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

        if (_playerControls.Player.Jump.WasPressedThisFrame())
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
            transform.localScale = new Vector2(-playerSpriteXScale, transform.localScale.y);
            _playerStateList.IsLookingRight = false;
            // transform.eulerAngles = new Vector3(0, 180, 0);
        }
        else if (xAxis > 0)
        {
            transform.localScale = new Vector2(playerSpriteXScale, transform.localScale.y);
            _playerStateList.IsLookingRight = true;
            // transform.eulerAngles = new Vector3(0, 0, 0);
        }
    }

    private void UpdateAnimationState()
    {
        string newState;

        // Determine which state to play based on game logic
        if (_playerStateList.IsAttacking)
        {
            // Use the attack animation that was determined when the attack started
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
        else if (_playerStateList.IsInvincible)
        {
            newState = STATE_HURT;
        }
        else
        {
            newState = STATE_IDLE;
        }

        // Only change state if it's different (prevents restarting the same animation)
        if (newState != _currentState)
        {
            _animator.Play(newState);
            _currentState = newState;
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
        _rigidbody2D.linearVelocity = new Vector2(transform.localScale.x * dashSpeed, 0);
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
        if (_playerControls.Player.Attack.WasPressedThisFrame() && timeSinceAttack >= timeBetweenAttacks)
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
                SlashEffectAtAngle(slashEffectWideVFXPrefab, 80 , UpAttackTransform);
            }
            else if (yAxis < 0 && !Grounded())
            {
                Hit(DownAttackTransform, DownAttackArea, ref _playerStateList.IsRecoilingYAxis, recoilYSpeed);
                SlashEffectAtAngle(slashEffectWideVFXPrefab, -90 , DownAttackTransform);
            }
        }
    }

    private void Hit(Transform attackTransform, Vector3 attackArea, ref bool recoilDir, float recoilStrength)
    {
        Collider2D[] objectsToHit = Physics2D.OverlapBoxAll(attackTransform.position, attackArea, 0, attackableLayer);

        if (objectsToHit.Length > 0)
        {
            recoilDir = true;
        }

        for (int i = 0; i < objectsToHit.Length; i++)
        {
            if (objectsToHit[i].GetComponent<EnemyCore>() != null)
            {
                EnemyCore enemy = objectsToHit[i].GetComponent<EnemyCore>();
                enemy.EnemyHit(damage, (transform.position - objectsToHit[i].transform.position).normalized, recoilStrength);
            }
        }
    }

    private void SlashEffectAtAngle(GameObject slashEffect, int effectAngle, Transform attackTransform)
    {
        slashEffect = Instantiate(slashEffect, attackTransform);
        slashEffect.transform.eulerAngles = new Vector3(0, 0, effectAngle);
        slashEffect.transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, 1);
    }

    private void Recoil()
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
        health -= Mathf.RoundToInt(damage);
        StartCoroutine(StopTakingDamageRoutine());
    }

    private void ClampHealth()
    {
        health = Mathf.Clamp(health, 0, maxHealth);
    }

    private IEnumerator StopTakingDamageRoutine()
    {
        _playerStateList.IsInvincible = true;
        ClampHealth();
        yield return new WaitForSeconds(invincibilityDuration);
        _playerStateList.IsInvincible = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(SideAttackTransform.position, SideAttackArea);
        Gizmos.DrawWireCube(UpAttackTransform.position, UpAttackArea);
        Gizmos.DrawWireCube(DownAttackTransform.position, DownAttackArea);
    }

    public int Health { get => health; set => health = value; }
    public int MaxHealth { get => maxHealth; set => maxHealth = value; }
}