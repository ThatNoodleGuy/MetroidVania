using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Shade : EnemyCore
{
    [SerializeField]
    private float chaseDistance;

    [SerializeField]
    private float stunDuration;

    private float timer;
    private bool isDying = false; // Prevent death logic from running multiple times

    public static Shade Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        Debug.Log("Shade spawned");

        // SaveData integration
        if (SaveData.Instance.sceneNames != null)
        {
            SaveData.Instance.SaveShadeData();
        }
    }

    protected override void Start()
    {
        base.Start();
        ChangeState(EnemyState.Shade_Idle);
    }

    protected override void Update()
    {
        base.Update();

        // Check if player is alive using PlayerStateList
        if (!PlayerController.Instance.GetComponent<PlayerStateList>().IsAlive)
        {
            ChangeState(EnemyState.Shade_Idle);
        }
    }

    protected override void UpdateEnemyStates()
    {
        float _dist = Vector2.Distance(
            transform.position,
            PlayerController.Instance.transform.position
        );

        switch (GetCurrentEnemyState)
        {
            case EnemyState.Shade_Idle:
                // Stop all movement when idle
                rb.linearVelocity = Vector2.zero;

                if (_dist < chaseDistance)
                {
                    ChangeState(EnemyState.Shade_Chase);
                }
                break;

            case EnemyState.Shade_Chase:
                // Use velocity instead of MovePosition for proper collision detection
                Vector2 direction = (
                    PlayerController.Instance.transform.position - transform.position
                ).normalized;
                rb.linearVelocity = direction * speed;

                Flip();

                if (_dist > chaseDistance)
                {
                    ChangeState(EnemyState.Shade_Idle);
                }
                break;

            case EnemyState.Shade_Stunned:
                // Stop movement during stun
                rb.linearVelocity = Vector2.zero;

                timer += Time.deltaTime;

                if (timer > stunDuration)
                {
                    ChangeState(EnemyState.Shade_Chase);
                    timer = 0;
                }
                break;

            case EnemyState.Shade_Death:
                Death(Random.Range(5f, 10f));
                break;
        }
    }

    public override void EnemyGetsHit(float damageDone, Vector2 hitDirection, float hitForce)
    {
        base.EnemyGetsHit(damageDone, hitDirection, hitForce);

        if (health > 0)
        {
            ChangeState(EnemyState.Shade_Stunned);
        }
        else
        {
            ChangeState(EnemyState.Shade_Death);
        }
    }

    protected override void Death(float _destroyTime)
    {
        rb.gravityScale = 12f;
        base.Death(_destroyTime);
    }

    protected override void ChangeCurrentAnimation()
    {
        // Make sure animator exists
        if (animator == null)
            return;

        // Idle animation
        if (GetCurrentEnemyState == EnemyState.Shade_Idle)
        {
            animator.Play("Player_Idle");
        }

        // Walking animation - set boolean parameter
        animator.SetBool("Walking", GetCurrentEnemyState == EnemyState.Shade_Chase);

        // Death animation and cleanup - ONLY RUN ONCE
        if (GetCurrentEnemyState == EnemyState.Shade_Death && !isDying)
        {
            isDying = true;

            // Restore player's mana when shade dies
            PlayerController.Instance.RestoreMana();

            // Save player data
            SaveData.Instance.SavePlayerData();

            // Trigger death animation
            animator.SetTrigger("Death");

            // Destroy shade after animation
            Destroy(gameObject, 0.5f);
        }
    }

    protected override void Attack()
    {
        // Make sure animator exists
        if (animator != null)
        {
            // Trigger attack animation
            animator.SetTrigger("Attacking");
        }

        // Deal damage to player
        PlayerController.Instance.TakeDamage(damage);
    }

    private void Flip()
    {
        // Flip sprite based on player position
        if (sr != null)
        {
            sr.flipX = PlayerController.Instance.transform.position.x < transform.position.x;
        }
    }
}
