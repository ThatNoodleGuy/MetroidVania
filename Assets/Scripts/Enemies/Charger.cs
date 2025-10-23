using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Charger : EnemyCore
{
    [SerializeField]
    private float ledgeCheckX;

    [SerializeField]
    private float ledgeCheckY;

    [SerializeField]
    private float chargeSpeedMultiplier;

    [SerializeField]
    private float chargeDuration;

    [SerializeField]
    private float jumpForce;

    [SerializeField]
    private LayerMask whatIsGround;

    private float timer;

    protected override void Start()
    {
        base.Start();
        ChangeState(EnemyState.Charger_Idle);
        rb.gravityScale = 12f;
    }

    protected override void Update()
    {
        base.Update();

        if (!PlayerController.Instance.GetComponent<PlayerStateList>().IsAlive)
        {
            ChangeState(EnemyState.Charger_Idle);
            return;
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.GetComponent<EnemyCore>())
        {
            transform.localScale = new Vector2(transform.localScale.x * -1, transform.localScale.y);
        }
    }

    protected override void UpdateEnemyStates()
    {
        if (health <= 0)
        {
            Death(0.1f);
        }

        Vector3 ledgeCheckStart =
            transform.localScale.x > 0 ? new Vector3(ledgeCheckX, 0) : new Vector3(-ledgeCheckX, 0);
        Vector2 wallCheckDir = transform.localScale.x > 0 ? transform.right : -transform.right;

        switch (GetCurrentEnemyState)
        {
            case EnemyState.Charger_Idle:
                // Idle behavior here

                if (
                    !Physics2D.Raycast(
                        transform.position + ledgeCheckStart,
                        Vector2.down,
                        ledgeCheckY,
                        whatIsGround
                    )
                    || Physics2D.Raycast(
                        transform.position,
                        wallCheckDir,
                        ledgeCheckX,
                        whatIsGround
                    )
                )
                {
                    transform.localScale = new Vector2(
                        transform.localScale.x * -1,
                        transform.localScale.y
                    );
                }

                RaycastHit2D hit = Physics2D.Raycast(
                    transform.position + ledgeCheckStart,
                    wallCheckDir,
                    ledgeCheckX * 10f
                );
                if (hit.collider != null && hit.collider.CompareTag("Player"))
                {
                    ChangeState(EnemyState.Charger_Surprised);
                }

                if (transform.localScale.x > 0)
                {
                    rb.linearVelocity = new Vector2(speed, rb.linearVelocity.y);
                }
                else
                {
                    rb.linearVelocity = new Vector2(-speed, rb.linearVelocity.y);
                }
                break;
            case EnemyState.Charger_Surprised:
                // Surprised behavior here
                rb.linearVelocity = new Vector2(0, jumpForce);
                ChangeState(EnemyState.Charger_Charge);
                break;
            case EnemyState.Charger_Charge:
                // Charge behavior here
                timer += Time.deltaTime;

                if (timer < chargeDuration)
                {
                    if (
                        Physics2D.Raycast(
                            transform.position,
                            Vector2.down,
                            ledgeCheckY,
                            whatIsGround
                        )
                    )
                    {
                        if (transform.localScale.x > 0)
                        {
                            rb.linearVelocity = new Vector2(
                                speed * chargeSpeedMultiplier,
                                rb.linearVelocity.y
                            );
                        }
                        else
                        {
                            rb.linearVelocity = new Vector2(
                                -speed * chargeSpeedMultiplier,
                                rb.linearVelocity.y
                            );
                        }
                    }
                    else
                    {
                        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                    }
                }
                else
                {
                    timer = 0f;
                    ChangeState(EnemyState.Charger_Idle);
                }
                break;

            default:
                break;
        }
    }

    protected override void ChangeCurrentAnimation()
    {
        if (GetCurrentEnemyState == EnemyState.Charger_Idle)
        {
            animator.speed = 1f;
        }

        if (GetCurrentEnemyState == EnemyState.Charger_Charge)
        {
            animator.speed = chargeSpeedMultiplier;
        }
    }
}
