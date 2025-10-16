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

    [Header("Horizontal Movement Settings")]
    [SerializeField] private float walkSpeed = 1f;
    [Space(5)]

    [Header("Vertical Movement Settings")]
    [SerializeField] private float jumpForce = 45f;
    private int jumpBufferCounter = 0;
    [SerializeField] private int jumpBufferFrames;
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
    [Space(5)]

    private PlayerControls _playerControls;
    private PlayerStateList _playerStateList;
    private Rigidbody2D _rigidbody2D;
    private Animator _animator;
    private float xAxis;
    private float playerSpriteXScale;
    private float _gravity;
    private string _currentState;
    private bool canDash = true;
    private bool dashed;

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

        if (_playerStateList.IsDashing)
        {
            UpdateAnimationState();
            return;
        }

        HandlePlayerSpriteFlip();
        HandleMovement();
        HandleJumping();
        HandleDashing();
        UpdateAnimationState();
    }

    private void UpdateAxisInput()
    {
        xAxis = MoveValue.x;
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
        if (_playerControls.Player.Jump.WasReleasedThisFrame() && _rigidbody2D.linearVelocity.y > 0)
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
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        if (_playerControls.Player.Jump.WasPressedThisFrame())
        {
            jumpBufferCounter = jumpBufferFrames;
        }
        else
        {
            jumpBufferCounter--;
        }
    }

    private void HandlePlayerSpriteFlip()
    {
        if (xAxis < 0)
        {
            transform.localScale = new Vector2(-playerSpriteXScale, transform.localScale.y);
            // transform.eulerAngles = new Vector3(0, 180, 0);
        }
        else if (xAxis > 0)
        {
            transform.localScale = new Vector2(playerSpriteXScale, transform.localScale.y);
            // transform.eulerAngles = new Vector3(0, 0, 0);
        }
    }

    private void UpdateAnimationState()
    {
        string newState;

        // Determine which state to play based on game logic
        if (_playerStateList.IsDashing)
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
}