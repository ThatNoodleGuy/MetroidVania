using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyCore : MonoBehaviour
{
    [SerializeField]
    protected float health;
    protected float maxHealth;

    [SerializeField]
    protected float recoilLength;

    [SerializeField]
    protected float recoilFactor;
    protected float recoilTimer;

    [SerializeField]
    protected bool isRecoiling = false;

    protected Rigidbody2D rb;
    protected SpriteRenderer sr;
    protected Animator animator;

    [SerializeField]
    protected PlayerController player;

    [SerializeField]
    protected float speed;

    [SerializeField]
    protected float damage;

    [SerializeField]
    protected GameObject orangeBloodVFXPrefab;

    [SerializeField]
    protected LayerMask whatIsIgnorePlayerLayer;

    protected enum EnemyState
    {
        // Crawler
        Crawler_Idle,
        Crawler_Flip,

        // Bat
        Bat_Idle,
        Bat_Chase,
        Bat_Stunned,
        Bat_Death,

        // Charger
        Charger_Idle,
        Charger_Surprised,
        Charger_Charge,

        // Shade
        Shade_Idle,
        Shade_Chase,
        Shade_Stunned,
        Shade_Death,

        // Add more enemy states as needed
    }

    protected EnemyState currentEnemyState;

    protected virtual EnemyState GetCurrentEnemyState
    {
        get { return currentEnemyState; }
        set
        {
            if (currentEnemyState != value)
            {
                currentEnemyState = value;

                ChangeCurrentAnimation();
            }
        }
    }

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        player = PlayerController.Instance;

        maxHealth = health;
    }

    protected virtual void Update()
    {
        if (health <= 0)
        {
            health = 0;
        }

        if (isRecoiling)
        {
            if (recoilTimer < recoilLength)
            {
                recoilTimer += Time.deltaTime;
            }
            else
            {
                isRecoiling = false;
                recoilTimer = 0;
            }
        }
        else
        {
            UpdateEnemyStates();
        }
    }

    public virtual void EnemyGetsHit(float damageDone, Vector2 hitDirection, float hitForce)
    {
        health -= damageDone;
        health = Mathf.Clamp(health, 0, maxHealth);

        if (!isRecoiling)
        {
            GameObject orangeBloodGO = Instantiate(
                orangeBloodVFXPrefab,
                transform.position,
                Quaternion.identity
            );
            Destroy(orangeBloodGO, 5.5f);
            rb.linearVelocity = -hitForce * recoilFactor * hitDirection;
            isRecoiling = true;
        }
    }

    protected void OnCollisionStay2D(Collision2D other)
    {
        if (
            other.gameObject.CompareTag("Player")
            && !PlayerController.Instance.GetComponent<PlayerStateList>().IsInvincible
            && health > 0
        )
        {
            Attack();
            if (PlayerController.Instance.GetComponent<PlayerStateList>().IsAlive)
            {
                PlayerController.Instance.HitStopTime(0, 5, 0.5f); // time scale = 0, restore speed = 5, delay = 0.5
            }
        }
    }

    protected virtual void Attack()
    {
        PlayerController.Instance.TakeDamage(damage);
    }

    public float Health
    {
        get => health;
        set => health = value;
    }
    public float RecoilLength
    {
        get => recoilLength;
        set => recoilLength = value;
    }
    public PlayerController Player
    {
        get => player;
        set => player = value;
    }

    protected virtual void UpdateEnemyStates() { }

    protected virtual void ChangeCurrentAnimation() { }

    protected virtual void ChangeState(EnemyState newState)
    {
        currentEnemyState = newState;
    }

    protected virtual void Death(float destroyTime)
    {
        gameObject.layer = whatIsIgnorePlayerLayer;

        Destroy(gameObject, destroyTime);
    }
}
